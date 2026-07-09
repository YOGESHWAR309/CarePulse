using System.Linq;
using System.Security.Claims;
using CarePulse.API.DTOs;
using CarePulse.API.Helpers;
using CarePulse.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoles.Patient)]
public class PatientAppointmentController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public PatientAppointmentController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet("available-slots/{doctorId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableSlots(int doctorId)
    {
        var slots = await _appointmentService.GetAvailableDoctorAvailabilitySlotsAsync(doctorId);
        return Ok(new
        {
            Message = "Available slots retrieved.",
            Slots = slots.Select(s => s.ToDto())
        });
    }

    [HttpGet("slots/{doctorId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSlots(int doctorId)
    {
        var slots = await _appointmentService.GetDoctorAvailabilitySlotsAsync(doctorId);
        return Ok(new
        {
            Message = "Doctor slots retrieved.",
            Slots = slots.Select(s => s.ToDto())
        });
    }

    [HttpPost("book-slot")]
    public async Task<IActionResult> BookSlot(AppointmentSlotBookingDto request)
    {
        var patientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var appointment = await _appointmentService.BookAppointmentSlotAsync(patientId, request);
        if (appointment is null)
            return BadRequest(new { Message = "Selected slot is unavailable or invalid." });

        return Ok(new
        {
            Message = "Appointment booked successfully.",
            Appointment = appointment.ToDto()
        });
    }
}
