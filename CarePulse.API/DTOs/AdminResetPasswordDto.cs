using System.ComponentModel.DataAnnotations;

namespace CarePulse.API.DTOs;

public class AdminResetPasswordDto
{
    [Required]
    [MinLength(6, ErrorMessage = "New password must be at least 6 characters long.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare("NewPassword", ErrorMessage = "Password and confirm password must match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
