namespace CarePulse.API.DTOs;

public class DoctorAvailabilitySlotDto
{
    public int DoctorAvailabilitySlotId { get; set; }
    public int DoctorId { get; set; }
    public DateTime SlotStart { get; set; }
    public DateTime SlotEnd { get; set; }
    public bool IsBooked { get; set; }
}
