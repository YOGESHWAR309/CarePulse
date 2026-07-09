using BCrypt.Net;
using CarePulse.API.DTOs;
using CarePulse.API.Helpers;
using CarePulse.API.Models;
using CarePulse.API.Repositories;
using CarePulse.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;
    private readonly ILogger<AdminController> _logger;
    private readonly IAuditRepository _auditRepository;

    public AdminController(IUserRepository userRepository, IAuthService authService, ILogger<AdminController> logger, IAuditRepository auditRepository)
    {
        _userRepository = userRepository;
        _authService = authService;
        _logger = logger;
        _auditRepository = auditRepository;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var users = (await _userRepository.GetAllUsersAsync()).ToList();
        var totalUsers = users.Count;
        var activeUsers = users.Count(x => x.IsActive);
        var inactiveUsers = totalUsers - activeUsers;
        var roleCounts = users
            .GroupBy(x => x.Role)
            .ToDictionary(g => g.Key, g => g.Count());

        return Ok(new
        {
            Message = "Admin dashboard access granted.",
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            InactiveUsers = inactiveUsers,
            RoleCounts = roleCounts,
            StaffCount = users.Count(x => x.Role != UserRoles.Patient),
            PatientCount = roleCounts.TryGetValue(UserRoles.Patient, out var patientCount) ? patientCount : 0,
            AdminCount = roleCounts.TryGetValue(UserRoles.Admin, out var adminCount) ? adminCount : 0
        });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("users/role")]
    public async Task<IActionResult> GetUsersByRole([FromQuery] string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return BadRequest(new { Message = "Role query is required." });

        var users = await _userRepository.GetUsersByRoleAsync(role);
        return Ok(users);
    }

    [HttpPost("create-staff")]
    public async Task<IActionResult> CreateStaff(CreateStaffDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (dto.Role == UserRoles.Admin || dto.Role == UserRoles.Patient)
        {
            return BadRequest(new { Message = "Only staff roles can be created through this endpoint." });
        }

        if (dto.Role == UserRoles.Doctor && string.IsNullOrWhiteSpace(dto.DoctorCategory))
        {
            ModelState.AddModelError(nameof(dto.DoctorCategory), "Doctor category is required for doctor accounts.");
        }

        if (dto.Role == UserRoles.Doctor && string.IsNullOrWhiteSpace(dto.DoctorQualification))
        {
            ModelState.AddModelError(nameof(dto.DoctorQualification), "Qualification is required for doctor accounts.");
        }

        if (dto.Role == UserRoles.Doctor && string.IsNullOrWhiteSpace(dto.DoctorSpecialty))
        {
            ModelState.AddModelError(nameof(dto.DoctorSpecialty), "Specialty is required for doctor accounts.");
        }

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser is not null)
        {
            return Conflict(new { Message = "Email is already registered." });
        }

        var newUser = new User
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim(),
            Phone = dto.Phone.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role.Trim(),
            DoctorCategory = dto.DoctorCategory?.Trim(),
            DoctorQualification = dto.DoctorQualification?.Trim(),
            DoctorSpecialty = dto.DoctorSpecialty?.Trim(),
            IsActive = true
        };

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUsers), new { id = newUser.UserId }, newUser);
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateStaffDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
            return NotFound(new { Message = "User not found." });

        if (dto.Role == UserRoles.Admin || dto.Role == UserRoles.Patient)
        {
            return BadRequest(new { Message = "Only staff roles can be assigned through this endpoint." });
        }

        user.FullName = dto.FullName.Trim();
        user.Email = dto.Email.Trim();
        user.Phone = dto.Phone.Trim();
        user.Role = dto.Role.Trim();
        user.IsActive = dto.IsActive;
        user.DoctorCategory = dto.DoctorCategory?.Trim();
        user.DoctorQualification = dto.DoctorQualification?.Trim();
        user.DoctorSpecialty = dto.DoctorSpecialty?.Trim();

        await _userRepository.UpdateUserAsync(user);
        await _userRepository.SaveChangesAsync();

        return Ok(user);
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
            return NotFound(new { Message = "User not found." });

        if (user.Role == UserRoles.Admin)
        {
            return BadRequest(new { Message = "Admin account cannot be deleted through this endpoint." });
        }

        var dependencyInfo = await _userRepository.GetUserDependencyInfoAsync(id);
        var hasDependencies = dependencyInfo.AppointmentCount > 0 || dependencyInfo.PrescriptionCount > 0 || dependencyInfo.DietPlanCount > 0 || dependencyInfo.MealOrderCount > 0 || dependencyInfo.RefreshTokenCount > 0 || dependencyInfo.PasswordResetTokenCount > 0 || dependencyInfo.PasswordResetOtpCount > 0;

        if (hasDependencies)
        {
            return BadRequest(new
            {
                Message = "User cannot be deleted because related data exists.",
                Dependencies = dependencyInfo
            });
        }

        await _userRepository.RemoveUserAsync(user);
        _logger?.LogInformation("Deleted user {UserId} by admin {Admin}", id, User?.Identity?.Name ?? "unknown");
        var actor = User?.Identity?.Name ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        await _auditRepository.AddAsync(new AuditLog
        {
            Actor = actor,
            Action = "DeleteUser",
            TargetUserId = id,
            Details = "User removed (no dependencies)."
        });
        await _auditRepository.SaveChangesAsync();

        await _userRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("users/{id}/dependency-info")]
    public async Task<IActionResult> GetUserDependencyInfo(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
            return NotFound(new { Message = "User not found." });

        var dependencyInfo = await _userRepository.GetUserDependencyInfoAsync(id);
        return Ok(dependencyInfo);
    }

    [HttpPut("users/{id}/status")]
    public async Task<IActionResult> SetUserActiveStatus(int id, [FromBody] bool isActive)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
            return NotFound(new { Message = "User not found." });

        user.IsActive = isActive;
        await _userRepository.UpdateUserAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger?.LogInformation("Set IsActive={IsActive} for user {UserId} by admin {Admin}", isActive, id, User?.Identity?.Name ?? "unknown");

        var actor = User?.Identity?.Name ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        await _auditRepository.AddAsync(new AuditLog
        {
            Actor = actor,
            Action = isActive ? "ActivateUser" : "DeactivateUser",
            TargetUserId = id,
            Details = $"Set IsActive={isActive} by admin."
        });
        await _auditRepository.SaveChangesAsync();

        return Ok(user);
    }

    [HttpPut("users/{id}/reset-password")]
    public async Task<IActionResult> ResetUserPassword(int id, [FromBody] AdminResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _authService.AdminResetPasswordAsync(id, dto);

        // record audit event for password reset
        var actor = User?.Identity?.Name ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        await _auditRepository.AddAsync(new AuditLog
        {
            Actor = actor,
            Action = "ResetUserPassword",
            TargetUserId = id,
            Details = "Admin-initiated password reset."
        });
        await _auditRepository.SaveChangesAsync();
        return response.Success ? Ok(response) : BadRequest(response);
    }
}