using CarePulse.API.DTOs;
using CarePulse.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _authService.RegisterAsync(dto);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _authService.LoginAsync(dto);
        return response.Success ? Ok(response) : Unauthorized(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _authService.RefreshTokenAsync(dto);
        return response.Success ? Ok(response) : Unauthorized(response);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _authService.ForgotPasswordAsync(dto);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _authService.ResetPasswordAsync(dto);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("verify-otp-and-reset")]
    public async Task<IActionResult> VerifyOtpAndResetPassword(VerifyOtpAndResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _authService.VerifyOtpAndResetPasswordAsync(dto);
        return response.Success ? Ok(response) : BadRequest(response);
    }
}
