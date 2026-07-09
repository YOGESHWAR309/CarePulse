using System.ComponentModel.DataAnnotations;

namespace CarePulse.API.DTOs;

public class AppointmentNoteDto
{
    [Required]
    [MinLength(3, ErrorMessage = "Notes must be at least 3 characters long.")]
    public string Notes { get; set; } = string.Empty;
}
