using CarePulse.API.DTOs;
using CarePulse.API.Models;
using CarePulse.API.Repositories;
using CarePulse.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Linq;

namespace CarePulse.API.Services;

public class AppointmentService : IAppointmentService
{
    private readonly ICareRepository _careRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IHubContext<NotificationsHub> _hubContext;

    public AppointmentService(
        ICareRepository careRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        ISmsService smsService,
        IHubContext<NotificationsHub> hubContext)
    {
        _careRepository = careRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _smsService = smsService;
        _hubContext = hubContext;
    }

    private static DateTime NormalizeToUtc(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
            return dateTime;

        if (dateTime.Kind == DateTimeKind.Local)
            return dateTime.ToUniversalTime();

        return DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime();
    }

    public async Task<Appointment> RequestAppointmentAsync(AppointmentRequestDto request)
    {
        return await ScheduleAppointmentAsync(request, "Requested");
    }

    public async Task<Appointment> ScheduleAppointmentAsync(AppointmentRequestDto request, string status)
    {
        var appointmentDateUtc = NormalizeToUtc(request.AppointmentDate);
        var appointment = new Appointment
        {
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            AppointmentDate = appointmentDateUtc,
            Notes = request.Notes,
            Status = status
        };

        await _careRepository.AddAppointmentAsync(appointment);
        await _careRepository.SaveChangesAsync();

        // broadcast new appointment
        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveAppointmentUpdate", appointment.ToDto());
        }
        catch { }

        return appointment;
    }

    public Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(int patientId)
    {
        return _careRepository.GetAppointmentsByPatientAsync(patientId);
    }

    public Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId)
    {
        return _careRepository.GetAppointmentsByDoctorAsync(doctorId);
    }

    public Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
    {
        return _careRepository.GetAllAppointmentsAsync();
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId)
    {
        return await _careRepository.GetAppointmentByIdAsync(appointmentId);
    }

    public async Task<Appointment?> UpdateAppointmentStatusAsync(int appointmentId, string status)
    {
        var appointment = await _careRepository.GetAppointmentByIdAsync(appointmentId);
        if (appointment is null)
            return null;

        appointment.Status = status;
        await _careRepository.SaveChangesAsync();

        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveAppointmentUpdate", appointment.ToDto());
        }
        catch { }

        return appointment;
    }

    public async Task<IEnumerable<DoctorAvailabilitySlot>> GetDoctorAvailabilitySlotsAsync(int doctorId)
    {
        return await _careRepository.GetDoctorAvailabilitySlotsAsync(doctorId);
    }

    public async Task<IEnumerable<DoctorAvailabilitySlot>> GetAvailableDoctorAvailabilitySlotsAsync(int doctorId)
    {
        return await _careRepository.GetAvailableDoctorAvailabilitySlotsAsync(doctorId);
    }

    public async Task<IEnumerable<DoctorAvailabilitySlot>> AddDoctorAvailabilitySlotsAsync(CreateDoctorAvailabilityDto request, int doctorId)
    {
        var slotStartUtc = NormalizeToUtc(request.SlotStart);
        var slotEndUtc = NormalizeToUtc(request.SlotEnd);
        var slotDuration = TimeSpan.FromMinutes(30);

        if (slotEndUtc <= slotStartUtc)
            throw new ArgumentException("Slot end time must be after start time.");

        if (slotStartUtc <= DateTime.UtcNow)
            throw new ArgumentException("Slot start time must be in the future.");

        if ((slotEndUtc - slotStartUtc).Ticks % slotDuration.Ticks != 0)
            throw new ArgumentException("Availability range must be a multiple of 30 minutes.");

        var existingSlots = await _careRepository.GetDoctorAvailabilitySlotsAsync(doctorId);
        if (existingSlots.Any(slot => slot.SlotStart < slotEndUtc && slotStartUtc < slot.SlotEnd))
            throw new ArgumentException("New availability range overlaps an existing slot.");

        var createdSlots = new List<DoctorAvailabilitySlot>();
        for (var currentStart = slotStartUtc; currentStart < slotEndUtc; currentStart += slotDuration)
        {
            var slot = new DoctorAvailabilitySlot
            {
                DoctorId = doctorId,
                SlotStart = currentStart,
                SlotEnd = currentStart.Add(slotDuration),
                IsBooked = false
            };
            await _careRepository.AddDoctorAvailabilitySlotAsync(slot);
            createdSlots.Add(slot);
        }

        await _careRepository.SaveChangesAsync();
        return createdSlots;
    }

    public async Task<IEnumerable<DoctorAvailabilitySlot>> AddDoctorWeeklyAvailabilitySlotsAsync(CreateDoctorWeeklyAvailabilityDto request, int doctorId)
    {
        var weekStartDate = request.WeekStart.Date;
        var daysToMonday = ((int)weekStartDate.DayOfWeek + 6) % 7;
        var weekMonday = weekStartDate.AddDays(-daysToMonday);
        var slotDuration = TimeSpan.FromMinutes(30);
        var createdSlots = new List<DoctorAvailabilitySlot>();

        if (!request.DaySchedules.Any(day => day.IsEnabled))
            throw new ArgumentException("At least one day must be enabled to create a weekly schedule.");

        var existingSlots = await _careRepository.GetDoctorAvailabilitySlotsAsync(doctorId);

        foreach (var daySchedule in request.DaySchedules.Where(day => day.IsEnabled))
        {
            if (string.IsNullOrWhiteSpace(daySchedule.StartTime) || string.IsNullOrWhiteSpace(daySchedule.EndTime))
                throw new ArgumentException($"Start and end times are required for {daySchedule.DayOfWeek}.");

            if (!TimeSpan.TryParse(daySchedule.StartTime, out var startTime))
                throw new ArgumentException($"Invalid start time for {daySchedule.DayOfWeek}.");

            if (!TimeSpan.TryParse(daySchedule.EndTime, out var endTime))
                throw new ArgumentException($"Invalid end time for {daySchedule.DayOfWeek}.");

            if (endTime <= startTime)
                throw new ArgumentException($"End time must be after start time for {daySchedule.DayOfWeek}.");

            if ((endTime - startTime).Ticks % slotDuration.Ticks != 0)
                throw new ArgumentException($"The schedule for {daySchedule.DayOfWeek} must be aligned to 30-minute increments.");

            var targetDate = weekMonday.AddDays(((int)daySchedule.DayOfWeek + 6) % 7);
            var dayStartUtc = NormalizeToUtc(targetDate.Add(startTime));
            var dayEndUtc = NormalizeToUtc(targetDate.Add(endTime));

            if (dayEndUtc <= DateTime.UtcNow)
                throw new ArgumentException($"Scheduled slots for {daySchedule.DayOfWeek} must be in the future.");

            for (var currentStart = dayStartUtc; currentStart < dayEndUtc; currentStart += slotDuration)
            {
                var newSlot = new DoctorAvailabilitySlot
                {
                    DoctorId = doctorId,
                    SlotStart = currentStart,
                    SlotEnd = currentStart.Add(slotDuration),
                    IsBooked = false
                };

                if (existingSlots.Any(existing => existing.SlotStart < newSlot.SlotEnd && newSlot.SlotStart < existing.SlotEnd))
                    throw new ArgumentException("New availability schedule overlaps an existing slot.");

                await _careRepository.AddDoctorAvailabilitySlotAsync(newSlot);
                createdSlots.Add(newSlot);
            }
        }

        await _careRepository.SaveChangesAsync();
        return createdSlots;
    }

    public async Task<Appointment?> BookAppointmentSlotAsync(int patientId, AppointmentSlotBookingDto request)
    {
        var slot = await _careRepository.GetDoctorAvailabilitySlotByIdAsync(request.SlotId);
        var slotStartUtc = slot is not null ? NormalizeToUtc(slot.SlotStart) : DateTime.MinValue;
        if (slot is null || slotStartUtc <= DateTime.UtcNow)
            return null;

        if (!await _careRepository.TryBookSlotAsync(request.SlotId))
            return null;

        slot.IsBooked = true;

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = slot.DoctorId,
            AppointmentDate = slotStartUtc,
            Notes = request.Notes,
            Status = "Scheduled"
        };

        await _careRepository.AddAppointmentAsync(appointment);
        await _careRepository.SaveChangesAsync();

        var patient = await _userRepository.GetByIdAsync(patientId);
        var doctor = await _userRepository.GetByIdAsync(slot.DoctorId);
        await NotifyAppointmentAsync(appointment, patient, doctor, "booked");

        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveAppointmentUpdate", appointment.ToDto());
        }
        catch { }

        return appointment;
    }

    public async Task<Appointment?> CancelAppointmentAsync(int appointmentId)
    {
        var appointment = await _careRepository.GetAppointmentByIdAsync(appointmentId);
        if (appointment is null)
            return null;

        // Free up the matching slot so someone else can book it
        var slots = await _careRepository.GetDoctorAvailabilitySlotsAsync(appointment.DoctorId);
        var matchingSlot = slots.FirstOrDefault(s => s.SlotStart == appointment.AppointmentDate);
        if (matchingSlot is not null)
        {
            matchingSlot.IsBooked = false;
            await _careRepository.UpdateDoctorAvailabilitySlotAsync(matchingSlot);
        }

        appointment.Status = "Cancelled";
        await _careRepository.SaveChangesAsync();

        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveAppointmentUpdate", appointment.ToDto());
        }
        catch { }

        return appointment;
    }

    public async Task<Appointment?> RescheduleAppointmentAsync(int appointmentId, DateTime newDate)
    {
        var appointment = await _careRepository.GetAppointmentByIdAsync(appointmentId);
        if (appointment is null || appointment.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            return null;

        var requestedStart = NormalizeToUtc(newDate);
        var availableSlots = await _careRepository.GetDoctorAvailabilitySlotsAsync(appointment.DoctorId);
        var targetSlot = availableSlots.FirstOrDefault(s => !s.IsBooked && s.SlotStart == requestedStart && s.SlotStart > DateTime.UtcNow);
        if (targetSlot is null)
            return null;

        return await RescheduleAppointmentAsync(appointmentId, targetSlot.DoctorAvailabilitySlotId);
    }

    public async Task<Appointment?> RescheduleAppointmentAsync(int appointmentId, int newSlotId)
    {
        var appointment = await _careRepository.GetAppointmentByIdAsync(appointmentId);
        if (appointment is null || appointment.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            return null;

        var newSlot = await _careRepository.GetDoctorAvailabilitySlotByIdAsync(newSlotId);
        if (newSlot is null || newSlot.SlotStart <= DateTime.UtcNow || newSlot.DoctorId != appointment.DoctorId)
            return null;

        if (!await _careRepository.TryBookSlotAsync(newSlotId))
            return null;

        var existingSlots = await _careRepository.GetDoctorAvailabilitySlotsAsync(appointment.DoctorId);
        var oldSlot = existingSlots.FirstOrDefault(s => s.SlotStart == appointment.AppointmentDate && s.IsBooked);
        if (oldSlot is not null)
        {
            oldSlot.IsBooked = false;
            await _careRepository.UpdateDoctorAvailabilitySlotAsync(oldSlot);
        }

        newSlot.IsBooked = true;
        await _careRepository.UpdateDoctorAvailabilitySlotAsync(newSlot);

        appointment.AppointmentDate = newSlot.SlotStart;
        appointment.Status = "Rescheduled";
        await _careRepository.SaveChangesAsync();

        var patient = await _userRepository.GetByIdAsync(appointment.PatientId);
        var doctor = await _userRepository.GetByIdAsync(appointment.DoctorId);
        await NotifyAppointmentAsync(appointment, patient, doctor, "rescheduled");

        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveAppointmentUpdate", appointment.ToDto());
        }
        catch { }

        return appointment;
    }

    public async Task<Appointment?> UpdateAppointmentNotesAsync(int appointmentId, string notes)
    {
        var appointment = await _careRepository.GetAppointmentByIdAsync(appointmentId);
        if (appointment is null || appointment.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            return null;

        appointment.Notes = notes;
        await _careRepository.SaveChangesAsync();

        var patient = await _userRepository.GetByIdAsync(appointment.PatientId);
        var doctor = await _userRepository.GetByIdAsync(appointment.DoctorId);
        await NotifyAppointmentAsync(appointment, patient, doctor, "updated notes for");

        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveAppointmentUpdate", appointment.ToDto());
        }
        catch { }

        return appointment;
    }

    public async Task<bool> SendAppointmentReminderAsync(int appointmentId)
    {
        var appointment = await _careRepository.GetAppointmentByIdAsync(appointmentId);
        if (appointment is null)
            return false;

        var patient = await _userRepository.GetByIdAsync(appointment.PatientId);
        var doctor = await _userRepository.GetByIdAsync(appointment.DoctorId);
        if (patient is null)
            return false;

        var subject = $"Reminder: Your appointment on {appointment.AppointmentDate.ToLocalTime():f}";
        var body = $"Hello {patient.FullName},<br/><br/>This is a reminder for your appointment with Dr. {doctor?.FullName ?? appointment.DoctorId.ToString()} on {appointment.AppointmentDate.ToLocalTime():f}.<br/><br/>Status: {appointment.Status}.<br/><br/>Notes: {appointment.Notes}<br/><br/>Thank you,<br/>CarePulse";

        await _emailService.SendEmailAsync(patient.Email, subject, body);
        if (!string.IsNullOrWhiteSpace(patient.Phone))
        {
            await _smsService.SendSmsAsync(patient.Phone, $"Reminder: Appointment on {appointment.AppointmentDate.ToLocalTime():f} with Dr. {doctor?.FullName ?? appointment.DoctorId.ToString()}.");
        }

        return true;
    }

    private async Task NotifyAppointmentAsync(Appointment appointment, User? patient, User? doctor, string action)
    {
        var appointmentDate = appointment.AppointmentDate.ToLocalTime();
        var doctorName = doctor?.FullName ?? $"Doctor {appointment.DoctorId}";
        var patientName = patient?.FullName ?? $"Patient {appointment.PatientId}";
        var notes = string.IsNullOrWhiteSpace(appointment.Notes) ? "No additional notes." : appointment.Notes;

        if (patient is not null)
        {
            var subject = $"CarePulse: Appointment {action}";
            var body = BuildAppointmentEmailBody(
                recipientName: patient.FullName,
                title: $"Your appointment has been {action}",
                message: $"Your appointment with {doctorName} has been {action}.",
                appointmentDate: appointmentDate,
                doctorName: doctorName,
                patientName: patientName,
                status: appointment.Status,
                notes: notes);

            await _emailService.SendEmailAsync(patient.Email, subject, body);
            if (!string.IsNullOrWhiteSpace(patient.Phone))
            {
                await _smsService.SendSmsAsync(patient.Phone, $"Your appointment with {doctorName} is on {appointmentDate:ddd, MMM d, yyyy h:mm tt} and is now {appointment.Status}.");
            }
        }

        if (doctor is not null)
        {
            var subject = $"CarePulse: Appointment {action} for {patientName}";
            var body = BuildAppointmentEmailBody(
                recipientName: doctor.FullName,
                title: $"Appointment {action} for your patient",
                message: $"The appointment with {patientName} has been {action}.",
                appointmentDate: appointmentDate,
                doctorName: doctorName,
                patientName: patientName,
                status: appointment.Status,
                notes: notes);

            await _emailService.SendEmailAsync(doctor.Email, subject, body);
        }
    }

    private static string BuildAppointmentEmailBody(string recipientName, string title, string message, DateTime appointmentDate, string doctorName, string patientName, string status, string notes)
    {
        return $@"
            <html>
              <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #223047; margin: 0; padding: 0;'>
                <div style='max-width: 640px; margin: 24px auto; padding: 24px; border: 1px solid #dce7f3; border-radius: 10px; background-color: #f9fcff;'>
                  <h2 style='margin-top: 0; color: #1976d2;'>{title}</h2>
                  <p>Hello {recipientName},</p>
                  <p>{message}</p>
                  <div style='background-color: #ffffff; border: 1px solid #e2ebf4; border-radius: 8px; padding: 16px; margin: 16px 0;'>
                    <p style='margin: 4px 0;'><strong>Patient:</strong> {patientName}</p>
                    <p style='margin: 4px 0;'><strong>Doctor:</strong> {doctorName}</p>
                    <p style='margin: 4px 0;'><strong>Date & Time:</strong> {appointmentDate:dddd, MMMM d, yyyy h:mm tt}</p>
                    <p style='margin: 4px 0;'><strong>Status:</strong> {status}</p>
                    <p style='margin: 4px 0;'><strong>Notes:</strong> {notes}</p>
                  </div>
                  <p>Thank you,<br/>CarePulse Healthcare Team</p>
                </div>
              </body>
            </html>";
    }
}
