using System.ComponentModel.DataAnnotations;

namespace CarePulse.API.DTOs;

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
