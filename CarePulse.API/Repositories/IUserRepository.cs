using CarePulse.API.DTOs;
using CarePulse.API.Models;

namespace CarePulse.API.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int userId);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
    Task AddAsync(User user);
    Task UpdateUserAsync(User user);
    Task RemoveUserAsync(User user);
    Task<UserDependencyInfoDto> GetUserDependencyInfoAsync(int userId);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task AddRefreshTokenAsync(RefreshToken refreshToken);
    Task RemoveRefreshTokenAsync(RefreshToken refreshToken);
    Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token);
    Task AddPasswordResetTokenAsync(PasswordResetToken resetToken);
    Task RemovePasswordResetTokenAsync(PasswordResetToken resetToken);
    Task<PasswordResetOtp?> GetPasswordResetOtpAsync(string email, string otp);
    Task AddPasswordResetOtpAsync(PasswordResetOtp otp);
    Task RemovePasswordResetOtpAsync(PasswordResetOtp otp);
    Task SaveChangesAsync();
}
