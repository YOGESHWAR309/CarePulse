using System.ComponentModel.DataAnnotations;

namespace CarePulse.API.DTOs;

public class SendOtpDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
