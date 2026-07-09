using System.ComponentModel.DataAnnotations;

namespace CarePulse.API.DTOs;

public class CreateDoctorWeeklyAvailabilityDto
{
    [Required]
    public DateTime WeekStart { get; set; }

    [Required]
    public List<WeeklyAvailabilityDayDto> DaySchedules { get; set; } = new();
}
