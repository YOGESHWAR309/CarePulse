using System.ComponentModel.DataAnnotations;
using CarePulse.API.Helpers;

namespace CarePulse.API.DTOs;

public class UpdateStaffDto
{
    [Required]
    [MinLength(3, ErrorMessage = "Full name must be at least 3 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [RegularExpression(UserRoles.StaffRoleValidationPattern, ErrorMessage = "Invalid staff role.")]
    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public string? DoctorCategory { get; set; }
    public string? DoctorQualification { get; set; }
    public string? DoctorSpecialty { get; set; }
}