using System.Linq;
using CarePulse.API.DTOs;
using CarePulse.API.Models;
using CarePulse.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestAppointment(AppointmentRequestDto request)
    {
        var appointment = await _appointmentService.RequestAppointmentAsync(request);
        return Ok(new
        {
            Message = "Appointment request submitted.",
            Appointment = appointment.ToDto()
        });
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetAppointmentsByPatient(int patientId)
    {
        var appointments = await _appointmentService.GetAppointmentsByPatientAsync(patientId);
        return Ok(appointments.Select(a => a.ToDto()));
    }

    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetAppointmentsByDoctor(int doctorId)
    {
        var appointments = await _appointmentService.GetAppointmentsByDoctorAsync(doctorId);
        return Ok(appointments.Select(a => a.ToDto()));
    }

    [HttpPut("{appointmentId}/status")]
    public async Task<IActionResult> UpdateStatus(int appointmentId, [FromBody] AppointmentStatusUpdateDto request)
    {
        // load the appointment to check authorization (who may change status)
        var appointmentEntity = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
        if (appointmentEntity is null)
            return NotFound(new { Message = "Appointment not found." });

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId))
            return Forbid();

        // Allow receptionist to update any appointment; allow doctor only for their own appointments
        if (userRole != null && userRole.Equals("Receptionist", StringComparison.OrdinalIgnoreCase))
        {
            // permitted
        }
        else if (userRole != null && userRole.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
        {
            if (appointmentEntity.DoctorId != currentUserId)
                return StatusCode(403, new { Message = "Only the assigned doctor may update this appointment." });
        }
        else if (userRole != null && userRole.Equals("Patient", StringComparison.OrdinalIgnoreCase))
        {
            if (appointmentEntity.PatientId != currentUserId)
                return StatusCode(403, new { Message = "Only the appointment owner may cancel this appointment." });
        }
        else
        {
            return Forbid();
        }

        var appointment = await _appointmentService.UpdateAppointmentStatusAsync(appointmentId, request.Status);
        if (appointment is null)
            return NotFound(new { Message = "Appointment not found." });

        return Ok(appointment.ToDto());
    }

    

    [HttpPost("{appointmentId}/reschedule")]
    public async Task<IActionResult> Reschedule(int appointmentId, AppointmentRescheduleDto request)
    {
        if (request is null || (request.NewSlotId == null && request.AppointmentDate == null))
            return BadRequest(new { Message = "A new slot or appointment date is required for rescheduling." });

        var appointmentEntity = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
        if (appointmentEntity is null)
            return NotFound(new { Message = "Appointment not found." });

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId))
            return Forbid();

        if (userRole != null && userRole.Equals("Receptionist", StringComparison.OrdinalIgnoreCase))
        {
            // permitted
        }
        else if (userRole != null && userRole.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
        {
            if (appointmentEntity.DoctorId != currentUserId)
                return StatusCode(403, new { Message = "Only the assigned doctor may reschedule this appointment." });
        }
        else if (userRole != null && userRole.Equals("Patient", StringComparison.OrdinalIgnoreCase))
        {
            if (appointmentEntity.PatientId != currentUserId)
                return StatusCode(403, new { Message = "Only the appointment owner may reschedule this appointment." });
        }
        else
        {
            return Forbid();
        }

        Appointment? appointment;
        if (request.NewSlotId.HasValue)
        {
            appointment = await _appointmentService.RescheduleAppointmentAsync(appointmentId, request.NewSlotId.Value);
        }
        else
        {
            appointment = await _appointmentService.RescheduleAppointmentAsync(appointmentId, request.AppointmentDate!.Value);
        }

        if (appointment is null)
            return BadRequest(new { Message = "The requested reschedule slot is unavailable or invalid." });

        return Ok(appointment.ToDto());
    }

    [HttpPut("{appointmentId}/notes")]
    public async Task<IActionResult> UpdateNotes(int appointmentId, AppointmentNoteDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var appointmentEntity = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
        if (appointmentEntity is null)
            return NotFound(new { Message = "Appointment not found." });

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId))
            return Forbid();

        if (userRole != null && userRole.Equals("Receptionist", StringComparison.OrdinalIgnoreCase))
        {
            // permitted
        }
        else if (userRole != null && userRole.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
        {
            if (appointmentEntity.DoctorId != currentUserId)
                return StatusCode(403, new { Message = "Only the assigned doctor may edit notes." });
        }
        else if (userRole != null && userRole.Equals("Patient", StringComparison.OrdinalIgnoreCase))
        {
            if (appointmentEntity.PatientId != currentUserId)
                return StatusCode(403, new { Message = "Only the appointment owner may edit notes." });
        }
        else
        {
            return Forbid();
        }

        var appointment = await _appointmentService.UpdateAppointmentNotesAsync(appointmentId, request.Notes);
        if (appointment is null)
            return BadRequest(new { Message = "Unable to update appointment notes." });

        return Ok(appointment.ToDto());
    }

    [HttpPost("{appointmentId}/remind")]
    public async Task<IActionResult> SendReminder(int appointmentId)
    {
        var appointmentEntity = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
        if (appointmentEntity is null)
            return NotFound(new { Message = "Appointment not found." });

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId))
            return Forbid();

        if (userRole != null && userRole.Equals("Receptionist", StringComparison.OrdinalIgnoreCase))
        {
            // permitted
        }
        else if (userRole != null && userRole.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
        {
            if (appointmentEntity.DoctorId != currentUserId)
                return StatusCode(403, new { Message = "Only the assigned doctor may send reminders." });
        }
        else if (userRole != null && userRole.Equals("Patient", StringComparison.OrdinalIgnoreCase))
        {
            if (appointmentEntity.PatientId != currentUserId)
                return StatusCode(403, new { Message = "Only the appointment owner may request reminders." });
        }
        else
        {
            return Forbid();
        }

        var sent = await _appointmentService.SendAppointmentReminderAsync(appointmentId);
        if (!sent)
            return BadRequest(new { Message = "Unable to send appointment reminder." });

        return Ok(new { Message = "Appointment reminder sent." });
    }

    [HttpPost("{appointmentId}/cancel")]
    public async Task<IActionResult> Cancel(int appointmentId)
    {
        var appointmentEntity = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
        if (appointmentEntity is null)
            return NotFound(new { Message = "Appointment not found." });

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId))
            return Forbid();

        if (userRole != null && userRole.Equals("Receptionist", StringComparison.OrdinalIgnoreCase))
        {
            // permitted
        }
        else if (userRole != null && userRole.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
        {
            if (appointmentEntity.DoctorId != currentUserId)
                return StatusCode(403, new { Message = "Only the assigned doctor may cancel this appointment." });
        }
        else if (userRole != null && userRole.Equals("Patient", StringComparison.OrdinalIgnoreCase))
        {
            if (appointmentEntity.PatientId != currentUserId)
                return StatusCode(403, new { Message = "Only the appointment owner may cancel this appointment." });
        }
        else
        {
            return Forbid();
        }

        var appointment = await _appointmentService.CancelAppointmentAsync(appointmentId);
        if (appointment is null)
            return NotFound(new { Message = "Appointment not found." });

        return Ok(appointment.ToDto());
    }
}
