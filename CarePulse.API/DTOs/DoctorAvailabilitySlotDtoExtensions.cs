using CarePulse.API.Models;

namespace CarePulse.API.DTOs;

public static class DoctorAvailabilitySlotDtoExtensions
{
    public static DoctorAvailabilitySlotDto ToDto(this DoctorAvailabilitySlot slot)
    {
        return new DoctorAvailabilitySlotDto
        {
            DoctorAvailabilitySlotId = slot.DoctorAvailabilitySlotId,
            DoctorId = slot.DoctorId,
            SlotStart = DateTime.SpecifyKind(slot.SlotStart, DateTimeKind.Utc).ToLocalTime(),
            SlotEnd = DateTime.SpecifyKind(slot.SlotEnd, DateTimeKind.Utc).ToLocalTime(),
            IsBooked = slot.IsBooked
        };
    }
}
