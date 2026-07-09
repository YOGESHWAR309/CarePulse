using System.ComponentModel.DataAnnotations;

namespace CarePulse.API.DTOs;

public class PrescriptionDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "PatientId must be a positive value.")]
    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    [Required]
    [MinLength(2, ErrorMessage = "Medication must be at least 2 characters long.")]
    public string Medication { get; set; } = string.Empty;

    public string Dosage { get; set; } = string.Empty;

    public string Instructions { get; set; } = string.Empty;
}
