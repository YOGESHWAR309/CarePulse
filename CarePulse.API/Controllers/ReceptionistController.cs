using CarePulse.API.DTOs;
using CarePulse.API.Helpers;
using CarePulse.API.Repositories;
using CarePulse.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoles.Receptionist)]
public class ReceptionistController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IUserRepository _userRepository;

    public ReceptionistController(IAppointmentService appointmentService, IUserRepository userRepository)
    {
        _appointmentService = appointmentService;
        _userRepository = userRepository;
    }

    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue()
    {
        var patients = await _userRepository.GetUsersByRoleAsync(UserRoles.Patient);
        var upcomingAppointments = await _appointmentService.GetAllAppointmentsAsync();

        return Ok(new
        {
            Message = "Receptionist queue access granted.",
            TotalPatients = patients.Count(),
            ActivePatients = patients.Count(x => x.IsActive),
            TodayAppointments = upcomingAppointments.Count(a => a.AppointmentDate.Date == DateTime.UtcNow.Date),
            WaitingDetails = new[]
            {
                new { Label = "New patient check-ins", Count = patients.Count(x => x.IsActive) },
                new { Label = "Today appointments", Count = upcomingAppointments.Count(a => a.AppointmentDate.Date == DateTime.UtcNow.Date) }
            }
        });
    }

    [HttpPost("token")]
    public IActionResult GenerateToken()
    {
        return Ok(new
        {
            Message = "Receptionist can generate visit tokens.",
            Token = Guid.NewGuid().ToString("N"),
            IssuedAt = DateTime.UtcNow
        });
    }

    [HttpGet("patients")]
    public async Task<IActionResult> GetWaitingPatients()
    {
        var patients = await _userRepository.GetUsersByRoleAsync(UserRoles.Patient);
        return Ok(new
        {
            Message = "Receptionist can view waiting patients.",
            Patients = patients.Select(p => new
            {
                PatientId = p.UserId,
                Name = p.FullName,
                Status = p.IsActive ? "Waiting" : "Inactive"
            })
        });
    }

    [HttpGet("doctors")]
    public async Task<IActionResult> GetAvailableDoctors()
    {
        var doctors = await _userRepository.GetUsersByRoleAsync(UserRoles.Doctor);
        return Ok(new
        {
            Message = "Receptionist can view doctors.",
            Doctors = doctors.Select(d => new
            {
                d.UserId,
                d.FullName,
                d.Email,
                d.Phone,
                d.DoctorCategory,
                d.DoctorQualification,
                d.DoctorSpecialty
            })
        });
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> GetReceptionistAppointments()
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        return Ok(new
        {
            Message = "Receptionist can view scheduled appointments.",
            Appointments = appointments.Select(a => new
            {
                a.AppointmentId,
                a.PatientId,
                PatientName = a.Patient?.FullName,
                a.DoctorId,
                DoctorName = a.Doctor?.FullName,
                a.AppointmentDate,
                a.Status,
                a.Notes
            })
        });
    }

    [HttpPost("appointments")]
    public async Task<IActionResult> CreateAppointment(AppointmentRequestDto request)
    {
        var appointment = await _appointmentService.ScheduleAppointmentAsync(request, "Scheduled");

        return Ok(new
        {
            Message = "Appointment scheduled successfully.",
            Appointment = new
            {
                appointment.AppointmentId,
                appointment.PatientId,
                appointment.DoctorId,
                appointment.AppointmentDate,
                appointment.Status,
                appointment.Notes
            }
        });
    }
}