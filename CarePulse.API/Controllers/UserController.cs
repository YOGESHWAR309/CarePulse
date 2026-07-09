using System.Security.Claims;
using BCrypt.Net;
using CarePulse.API.DTOs;
using CarePulse.API.Helpers;
using CarePulse.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : 0;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ServiceResponse<object>>> GetProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized(new ServiceResponse<object> { Success = false, Message = "Invalid user." });

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFound(new ServiceResponse<object> { Success = false, Message = "User not found." });

        // Expose non-sensitive fields
        var dto = new
        {
            user.UserId,
            user.FullName,
            user.Email,
            user.Phone,
            user.Role,
            user.IsActive,
            user.DoctorCategory,
            user.DoctorQualification,
            user.DoctorSpecialty,
            user.CreatedAt
        };

        return Ok(new ServiceResponse<object> { Data = dto });
    }

    [HttpPut("me")]
    public async Task<ActionResult<ServiceResponse<object>>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized(new ServiceResponse<object> { Success = false, Message = "Invalid user." });

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFound(new ServiceResponse<object> { Success = false, Message = "User not found." });

        user.FullName = dto.FullName.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Phone))
            user.Phone = dto.Phone.Trim();

        await _userRepository.UpdateUserAsync(user);
        await _userRepository.SaveChangesAsync();

        return Ok(new ServiceResponse<object> { Data = "Profile updated successfully." });
    }

    [HttpPut("me/change-password")]
    public async Task<ActionResult<ServiceResponse<string>>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
            return BadRequest(new ServiceResponse<string> { Success = false, Message = "New password and confirmation do not match." });

        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized(new ServiceResponse<string> { Success = false, Message = "Invalid user." });

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFound(new ServiceResponse<string> { Success = false, Message = "User not found." });

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return BadRequest(new ServiceResponse<string> { Success = false, Message = "Current password is incorrect." });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepository.UpdateUserAsync(user);
        await _userRepository.SaveChangesAsync();

        return Ok(new ServiceResponse<string> { Data = "Password changed successfully." });
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ServiceResponse<string>>> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        var existing = await _userRepository.GetRefreshTokenAsync(dto.RefreshToken);
        if (existing is null)
            return Ok(new ServiceResponse<string> { Data = "Logged out." });

        await _userRepository.RemoveRefreshTokenAsync(existing);
        await _userRepository.SaveChangesAsync();

        return Ok(new ServiceResponse<string> { Data = "Logged out and refresh token revoked." });
    }

    [HttpGet("dependencies")]
    public async Task<ActionResult<ServiceResponse<UserDependencyInfoDto>>> GetDependencies()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized(new ServiceResponse<UserDependencyInfoDto> { Success = false, Message = "Invalid user." });

        var deps = await _userRepository.GetUserDependencyInfoAsync(userId);
        return Ok(new ServiceResponse<UserDependencyInfoDto> { Data = deps });
    }
}
