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
public class DoctorAvailabilityController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public DoctorAvailabilityController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet("slots")]
    [Authorize(Roles = UserRoles.Doctor)]
    public async Task<IActionResult> GetSlots()
    {
        var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var slots = await _appointmentService.GetDoctorAvailabilitySlotsAsync(doctorId);
        return Ok(new
        {
            Message = "Doctor availability slots retrieved.",
            Slots = slots.Select(s => s.ToDto())
        });
    }

    [HttpGet("available/{doctorId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableSlots(int doctorId)
    {
        var slots = await _appointmentService.GetAvailableDoctorAvailabilitySlotsAsync(doctorId);
        return Ok(new
        {
            Message = "Available doctor slots retrieved.",
            Slots = slots.Select(s => s.ToDto())
        });
    }

    [HttpPost("slots")]
    [Authorize(Roles = UserRoles.Doctor)]
    public async Task<IActionResult> CreateSlot(CreateDoctorAvailabilityDto request)
    {
        var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        try
        {
            var slots = await _appointmentService.AddDoctorAvailabilitySlotsAsync(request, doctorId);
            return Ok(new
            {
                Message = "Doctor availability slots created.",
                Slots = slots.Select(s => s.ToDto())
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("weekly-slots")]
    [Authorize(Roles = UserRoles.Doctor)]
    public async Task<IActionResult> CreateWeeklySlots(CreateDoctorWeeklyAvailabilityDto request)
    {
        var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        try
        {
            var slots = await _appointmentService.AddDoctorWeeklyAvailabilitySlotsAsync(request, doctorId);
            return Ok(new
            {
                Message = "Doctor weekly availability slots created.",
                Slots = slots.Select(s => s.ToDto())
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
