namespace CarePulse.API.DTOs;

public class LoginResponseDto
{
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime TokenExpiresAt { get; set; }
}