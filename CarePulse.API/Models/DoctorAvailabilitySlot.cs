namespace CarePulse.API.Models;

public class DoctorAvailabilitySlot
{
    public int DoctorAvailabilitySlotId { get; set; }
    public int DoctorId { get; set; }
    public DateTime SlotStart { get; set; }
    public DateTime SlotEnd { get; set; }
    public bool IsBooked { get; set; }

    public User? Doctor { get; set; }
}
