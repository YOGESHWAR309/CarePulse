using System.ComponentModel.DataAnnotations;
using CarePulse.API.Helpers;

namespace CarePulse.API.DTOs;

public class CreateStaffDto
{
    [Required]
    [MinLength(3, ErrorMessage = "Full name must be at least 3 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [RegularExpression(UserRoles.StaffRoleValidationPattern, ErrorMessage = "Invalid staff role.")]
    public string Role { get; set; } = string.Empty;

    public string? DoctorCategory { get; set; }
    public string? DoctorQualification { get; set; }
    public string? DoctorSpecialty { get; set; }
}