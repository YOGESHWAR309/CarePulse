using CarePulse.API.Models;

namespace CarePulse.API.DTOs;

public class AppointmentDto
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public UserSummaryDto? Patient { get; set; }
    public UserSummaryDto? Doctor { get; set; }
}

public class UserSummaryDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
}

public static class AppointmentDtoExtensions
{
    public static AppointmentDto ToDto(this Appointment appointment)
    {
        return new AppointmentDto
        {
            AppointmentId = appointment.AppointmentId,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            AppointmentDate = appointment.AppointmentDate,
            Status = appointment.Status,
            Notes = appointment.Notes,
            CreatedAt = appointment.CreatedAt,
            Patient = appointment.Patient is null ? null : new UserSummaryDto
            {
                UserId = appointment.Patient.UserId,
                FullName = appointment.Patient.FullName
            },
            Doctor = appointment.Doctor is null ? null : new UserSummaryDto
            {
                UserId = appointment.Doctor.UserId,
                FullName = appointment.Doctor.FullName
            }
        };
    }
}
