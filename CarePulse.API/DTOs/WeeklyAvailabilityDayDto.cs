using System.ComponentModel.DataAnnotations;

namespace CarePulse.API.DTOs;

public class WeeklyAvailabilityDayDto
{
    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    public bool IsEnabled { get; set; }

    public string? StartTime { get; set; }

    public string? EndTime { get; set; }
}
