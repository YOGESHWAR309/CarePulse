using CarePulse.API.Data;
using CarePulse.API.DTOs;
using CarePulse.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbContext.Users
            .Include(x => x.RefreshTokens)
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email.Trim().ToLower());
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _dbContext.Users
            .Include(x => x.RefreshTokens)
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task AddAsync(User user)
    {
        await _dbContext.Users.AddAsync(user);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _dbContext.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken);
    }

    public Task RemoveRefreshTokenAsync(RefreshToken refreshToken)
    {
        _dbContext.RefreshTokens.Remove(refreshToken);
        return Task.CompletedTask;
    }

    public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token)
    {
        return await _dbContext.PasswordResetTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token);
    }

    public async Task AddPasswordResetTokenAsync(PasswordResetToken resetToken)
    {
        await _dbContext.PasswordResetTokens.AddAsync(resetToken);
    }

    public Task RemovePasswordResetTokenAsync(PasswordResetToken resetToken)
    {
        _dbContext.PasswordResetTokens.Remove(resetToken);
        return Task.CompletedTask;
    }

    public async Task<PasswordResetOtp?> GetPasswordResetOtpAsync(string email, string otp)
    {
        return await _dbContext.PasswordResetOtps
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email.Trim().ToLower() && x.Otp == otp && !x.IsUsed && x.ExpiryDate > DateTime.UtcNow);
    }

    public async Task AddPasswordResetOtpAsync(PasswordResetOtp otp)
    {
        await _dbContext.PasswordResetOtps.AddAsync(otp);
    }

    public Task RemovePasswordResetOtpAsync(PasswordResetOtp otp)
    {
        _dbContext.PasswordResetOtps.Remove(otp);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _dbContext.Users.ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
    {
        return await _dbContext.Users
            .Where(x => x.Role == role)
            .ToListAsync();
    }

    public Task UpdateUserAsync(User user)
    {
        _dbContext.Users.Update(user);
        return Task.CompletedTask;
    }

    public Task RemoveUserAsync(User user)
    {
        _dbContext.Users.Remove(user);
        return Task.CompletedTask;
    }

    public async Task<UserDependencyInfoDto> GetUserDependencyInfoAsync(int userId)
    {
        var appointmentCount = await _dbContext.Appointments
            .CountAsync(x => x.PatientId == userId || x.DoctorId == userId);

        var prescriptionCount = await _dbContext.Prescriptions
            .CountAsync(x => x.PatientId == userId || x.DoctorId == userId);

        var dietPlanCount = await _dbContext.DietPlans
            .CountAsync(x => x.PatientId == userId || x.CreatedById == userId);

        var mealOrderCount = await _dbContext.MealOrders
            .CountAsync(x => x.PatientId == userId);

        var refreshTokenCount = await _dbContext.RefreshTokens
            .CountAsync(x => x.UserId == userId);

        var passwordResetTokenCount = await _dbContext.PasswordResetTokens
            .CountAsync(x => x.UserId == userId);

        var passwordResetOtpCount = await _dbContext.PasswordResetOtps
            .CountAsync(x => x.UserId == userId);

        return new UserDependencyInfoDto
        {
            AppointmentCount = appointmentCount,
            PrescriptionCount = prescriptionCount,
            DietPlanCount = dietPlanCount,
            MealOrderCount = mealOrderCount,
            RefreshTokenCount = refreshTokenCount,
            PasswordResetTokenCount = passwordResetTokenCount,
            PasswordResetOtpCount = passwordResetOtpCount
        };
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
