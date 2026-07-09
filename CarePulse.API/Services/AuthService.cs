using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using CarePulse.API.DTOs;
using CarePulse.API.Helpers;
using CarePulse.API.Models;
using CarePulse.API.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CarePulse.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUserRepository userRepository,
        IEmailService emailService,
        ISmsService smsService,
        IOptions<JwtSettings> jwtOptions)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _smsService = smsService;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<ServiceResponse<LoginResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        if (registerDto.Password != registerDto.ConfirmPassword)
        {
            return new ServiceResponse<LoginResponseDto>
            {
                Success = false,
                Message = "Password and confirm password do not match."
            };
        }

        var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
        if (existingUser is not null)
        {
            return new ServiceResponse<LoginResponseDto>
            {
                Success = false,
                Message = "Email is already registered."
            };
        }

        var user = new User
        {
            FullName = registerDto.FullName.Trim(),
            Email = registerDto.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            Phone = registerDto.Phone.Trim(),
            Role = UserRoles.Patient,
            IsActive = true
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Send welcome SMS if phone is present — best-effort, do not block registration on failure
        try
        {
            if (!string.IsNullOrWhiteSpace(user.Phone))
            {
                var welcome = $"Welcome to CarePulse, {user.FullName}. Your account has been created.";
                _ = _smsService.SendSmsAsync(user.Phone, welcome);
            }
        }
        catch
        {
            // swallow exceptions to avoid failing registration due to SMS issues
        }

        var refreshToken = GenerateRefreshToken(user.UserId);
        await _userRepository.AddRefreshTokenAsync(refreshToken);
        await _userRepository.SaveChangesAsync();

        return new ServiceResponse<LoginResponseDto>
        {
            Data = CreateAuthResponse(user, refreshToken.Token)
        };
    }

    public async Task<ServiceResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user is null || !user.IsActive)
        {
            return new ServiceResponse<LoginResponseDto>
            {
                Success = false,
                Message = "Invalid credentials."
            };
        }

        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return new ServiceResponse<LoginResponseDto>
            {
                Success = false,
                Message = "Invalid credentials."
            };
        }

        var refreshToken = GenerateRefreshToken(user.UserId);
        await _userRepository.AddRefreshTokenAsync(refreshToken);
        await _userRepository.SaveChangesAsync();

        return new ServiceResponse<LoginResponseDto>
        {
            Data = CreateAuthResponse(user, refreshToken.Token)
        };
    }

    public async Task<ServiceResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto requestDto)
    {
        var existingToken = await _userRepository.GetRefreshTokenAsync(requestDto.RefreshToken);
        if (existingToken is null || existingToken.ExpiryDate <= DateTime.UtcNow)
        {
            return new ServiceResponse<LoginResponseDto>
            {
                Success = false,
                Message = "Refresh token is invalid or has expired."
            };
        }

        if (existingToken.User is null)
        {
            return new ServiceResponse<LoginResponseDto>
            {
                Success = false,
                Message = "Refresh token is invalid."
            };
        }

        await _userRepository.RemoveRefreshTokenAsync(existingToken);

        var refreshToken = GenerateRefreshToken(existingToken.UserId);
        await _userRepository.AddRefreshTokenAsync(refreshToken);
        await _userRepository.SaveChangesAsync();

        return new ServiceResponse<LoginResponseDto>
        {
            Data = CreateAuthResponse(existingToken.User, refreshToken.Token)
        };
    }

    public async Task<ServiceResponse<string>> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user is null)
        {
            // Don't reveal if email exists or not (security best practice)
            return new ServiceResponse<string>
            {
                Data = "",
                Message = "If an account with this email exists, a password reset OTP has been sent."
            };
        }

        // Generate 6-digit OTP
        var otp = Random.Shared.Next(100000, 999999).ToString();

        var passwordOtp = new PasswordResetOtp
        {
            UserId = user.UserId,
            Otp = otp,
            Email = user.Email,
            ExpiryDate = DateTime.UtcNow.AddMinutes(15)
        };

        await _userRepository.AddPasswordResetOtpAsync(passwordOtp);
        await _userRepository.SaveChangesAsync();

        // Send OTP via email
        var emailSent = await _emailService.SendPasswordResetOtpEmailAsync(user.Email, user.FullName, otp);

        return new ServiceResponse<string>
        {
            Data = "",
            Message = emailSent
                ? "Password reset OTP has been sent to your email. It will expire in 15 minutes."
                : "OTP email could not be sent. Please try again later or contact support."
        };
    }

    public async Task<ServiceResponse<string>> VerifyOtpAndResetPasswordAsync(VerifyOtpAndResetPasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
        {
            return new ServiceResponse<string>
            {
                Success = false,
                Message = "Password and confirmation do not match."
            };
        }

        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user is null)
        {
            return new ServiceResponse<string>
            {
                Success = false,
                Message = "Invalid email address."
            };
        }

        var validOtp = await _userRepository.GetPasswordResetOtpAsync(dto.Email, dto.Otp);
        if (validOtp is null)
        {
            return new ServiceResponse<string>
            {
                Success = false,
                Message = "Invalid or expired OTP. Please request a new one."
            };
        }

        // Mark OTP as used
        validOtp.IsUsed = true;

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        await _userRepository.UpdateUserAsync(user);
        await _userRepository.SaveChangesAsync();

        return new ServiceResponse<string>
        {
            Data = "Password has been reset successfully.",
            Message = "Your password has been updated. You can now log in with your new password."
        };
    }

    public async Task<ServiceResponse<string>> ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
        {
            return new ServiceResponse<string>
            {
                Success = false,
                Message = "New password and confirmation do not match."
            };
        }

        var resetToken = await _userRepository.GetPasswordResetTokenAsync(dto.Token);
        if (resetToken is null || resetToken.ExpiryDate <= DateTime.UtcNow)
        {
            return new ServiceResponse<string>
            {
                Success = false,
                Message = "Password reset token is invalid or has expired."
            };
        }

        if (resetToken.User is null)
        {
            return new ServiceResponse<string>
            {
                Success = false,
                Message = "Password reset token is invalid."
            };
        }

        resetToken.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepository.RemovePasswordResetTokenAsync(resetToken);
        await _userRepository.SaveChangesAsync();

        return new ServiceResponse<string>
        {
            Data = "Password has been reset successfully."
        };
    }

    public async Task<ServiceResponse<string>> AdminResetPasswordAsync(int userId, AdminResetPasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
        {
            return new ServiceResponse<string>
            {
                Success = false,
                Message = "New password and confirmation do not match."
            };
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return new ServiceResponse<string>
            {
                Success = false,
                Message = "User not found."
            };
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepository.UpdateUserAsync(user);
        await _userRepository.SaveChangesAsync();

        return new ServiceResponse<string>
        {
            Data = $"Password for {user.FullName} has been reset successfully.",
            Message = $"Password for user '{user.Email}' has been reset. They can now log in with the new password."
        };
    }

    private LoginResponseDto CreateAuthResponse(User user, string refreshToken)
    {
        var (token, expiresAt) = CreateJwtToken(user);

        return new LoginResponseDto
        {
            UserId = user.UserId,
            Token = token,
            RefreshToken = refreshToken,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            TokenExpiresAt = expiresAt
        };
    }

    private (string Token, DateTime ExpiresAt) CreateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    private RefreshToken GenerateRefreshToken(int userId)
    {
        return new RefreshToken
        {
            UserId = userId,
            Token = Guid.NewGuid().ToString("N"),
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays)
        };
    }
}
