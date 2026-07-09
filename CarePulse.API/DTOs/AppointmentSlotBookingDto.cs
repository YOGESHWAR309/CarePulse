using System.ComponentModel.DataAnnotations;

namespace CarePulse.API.DTOs;

public class AppointmentSlotBookingDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "SlotId must be a positive value.")]
    public int SlotId { get; set; }

    public string Notes { get; set; } = string.Empty;
}
