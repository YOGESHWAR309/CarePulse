using System.ComponentModel.DataAnnotations;

namespace CarePulse.API.DTOs;

public class AppointmentRequestDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "PatientId must be a positive value.")]
    public int PatientId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "DoctorId must be a positive value.")]
    public int DoctorId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }

    public string Notes { get; set; } = string.Empty;
}
