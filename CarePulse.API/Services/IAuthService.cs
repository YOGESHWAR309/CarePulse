using CarePulse.API.DTOs;
using CarePulse.API.Helpers;

namespace CarePulse.API.Services;

public interface IAuthService
{
    Task<ServiceResponse<LoginResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<ServiceResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto);
    Task<ServiceResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto requestDto);
    Task<ServiceResponse<string>> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<ServiceResponse<string>> VerifyOtpAndResetPasswordAsync(VerifyOtpAndResetPasswordDto dto);
    Task<ServiceResponse<string>> ResetPasswordAsync(ResetPasswordDto dto);
    Task<ServiceResponse<string>> AdminResetPasswordAsync(int userId, AdminResetPasswordDto dto);
}
