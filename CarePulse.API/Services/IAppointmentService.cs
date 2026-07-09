using CarePulse.API.DTOs;
using CarePulse.API.Models;

namespace CarePulse.API.Services;

public interface IAppointmentService
{
    Task<Appointment> RequestAppointmentAsync(AppointmentRequestDto request);
    Task<Appointment> ScheduleAppointmentAsync(AppointmentRequestDto request, string status);
    Task<IEnumerable<DoctorAvailabilitySlot>> GetDoctorAvailabilitySlotsAsync(int doctorId);
    Task<IEnumerable<DoctorAvailabilitySlot>> GetAvailableDoctorAvailabilitySlotsAsync(int doctorId);
    Task<IEnumerable<DoctorAvailabilitySlot>> AddDoctorAvailabilitySlotsAsync(CreateDoctorAvailabilityDto request, int doctorId);
    Task<IEnumerable<DoctorAvailabilitySlot>> AddDoctorWeeklyAvailabilitySlotsAsync(CreateDoctorWeeklyAvailabilityDto request, int doctorId);
    Task<Appointment?> BookAppointmentSlotAsync(int patientId, AppointmentSlotBookingDto request);
    Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(int patientId);
    Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId);
    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
    Task<Appointment?> GetAppointmentByIdAsync(int appointmentId);
    Task<Appointment?> UpdateAppointmentStatusAsync(int appointmentId, string status);
    Task<Appointment?> RescheduleAppointmentAsync(int appointmentId, DateTime newDate);
    Task<Appointment?> RescheduleAppointmentAsync(int appointmentId, int newSlotId);
    Task<Appointment?> UpdateAppointmentNotesAsync(int appointmentId, string notes);
    Task<bool> SendAppointmentReminderAsync(int appointmentId);
    Task<Appointment?> CancelAppointmentAsync(int appointmentId);
}
