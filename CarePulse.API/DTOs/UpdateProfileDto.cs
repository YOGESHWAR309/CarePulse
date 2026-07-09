using System.ComponentModel.DataAnnotations;

namespace CarePulse.API.DTOs;

public class UpdateProfileDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }
}
