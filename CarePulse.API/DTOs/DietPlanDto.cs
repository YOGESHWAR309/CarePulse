using System.ComponentModel.DataAnnotations;

namespace CarePulse.API.DTOs;

public class DietPlanDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "PatientId must be a positive value.")]
    public int PatientId { get; set; }

    [Required]
    [MinLength(5, ErrorMessage = "Diet summary must be at least 5 characters long.")]
    public string DietSummary { get; set; } = string.Empty;

    [Required]
    [MinLength(3, ErrorMessage = "Goal must be at least 3 characters long.")]
    public string Goal { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;
}
