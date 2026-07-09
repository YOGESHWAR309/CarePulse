using System.ComponentModel.DataAnnotations;

namespace CarePulse.API.DTOs;

public class AppointmentStatusUpdateDto
{
    [Required]
    [MinLength(3, ErrorMessage = "Status must be at least 3 characters long.")]
    public string Status { get; set; } = string.Empty;
}
