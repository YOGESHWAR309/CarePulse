using System.ComponentModel.DataAnnotations;

namespace CarePulse.API.DTOs;

public class AppointmentRescheduleDto
{
        public int? NewSlotId { get; set; }
    public DateTime? AppointmentDate { get; set; }
}