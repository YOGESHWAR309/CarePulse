using System.ComponentModel.DataAnnotations;

namespace CarePulse.API.DTOs;

public class CreateDoctorAvailabilityDto
{
    [Required]
    public DateTime SlotStart { get; set; }

    [Required]
    public DateTime SlotEnd { get; set; }
}
