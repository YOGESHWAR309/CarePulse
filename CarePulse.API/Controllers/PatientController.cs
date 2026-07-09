using System.Linq;
using System.Security.Claims;
using CarePulse.API.DTOs;
using CarePulse.API.Helpers;
using CarePulse.API.Repositories;
using CarePulse.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoles.Patient)]
public class PatientController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IMealService _mealService;
    private readonly IUserRepository _userRepository;

    public PatientController(
        IAppointmentService appointmentService,
        IMealService mealService,
        IUserRepository userRepository)
    {
        _appointmentService = appointmentService;
        _mealService = mealService;
        _userRepository = userRepository;
    }

    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        return Ok(new
        {
            Message = "Patient profile access granted.",
            Data = new
            {
                PatientId = 0,
                FullName = "Sample Patient",
                Email = "patient@example.com"
            }
        });
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointments()
    {
        var patientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var appointments = await _appointmentService.GetAppointmentsByPatientAsync(patientId);
        return Ok(new
        {
            Message = "Patient can view appointments.",
            Appointments = appointments.Select(a => a.ToDto())
        });
    }

    [HttpPost("appointments")]
    public async Task<IActionResult> RequestAppointment(AppointmentRequestDto request)
    {
        var patientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        request.PatientId = patientId;
        var appointment = await _appointmentService.RequestAppointmentAsync(request);

        return Ok(new
        {
            Message = "Appointment request submitted.",
            Appointment = appointment.ToDto()
        });
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetMealOrders()
    {
        var patientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var orders = await _mealService.GetMealOrdersByPatientAsync(patientId);
        return Ok(new
        {
            Message = "Patient can view meal orders.",
            Orders = orders
        });
    }

    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctors()
    {
        var doctors = await _userRepository.GetUsersByRoleAsync(UserRoles.Doctor);
        return Ok(new
        {
            Message = "Patient can view doctors.",
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

    [HttpPost("orders")]
    public async Task<IActionResult> PlaceMealOrder(MealOrderRequestDto request)
    {
        var patientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        request.PatientId = patientId;
        var order = await _mealService.PlaceMealOrderAsync(request);

        return Ok(new
        {
            Message = "Meal order placed successfully.",
            Order = order
        });
    }

    [HttpPut("orders/{orderId}/activate")]
    public async Task<IActionResult> ActivateMealOrder(int orderId)
    {
        var patientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        
        // Verify the order belongs to the patient
        var order = await _mealService.GetMealOrderByIdAsync(orderId);
        if (order is null || order.PatientId != patientId)
            return Unauthorized(new { Message = "You can only activate your own orders." });

        var updatedOrder = await _mealService.UpdateMealOrderStatusAsync(orderId, "Confirmed");
        if (updatedOrder is null)
            return NotFound(new { Message = "Order not found." });

        return Ok(new
        {
            Message = "Meal order activated successfully.",
            Order = updatedOrder
        });
    }

    [HttpPut("orders/{orderId}/reject")]
    public async Task<IActionResult> RejectMealOrder(int orderId)
    {
        var patientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        
        // Verify the order belongs to the patient
        var order = await _mealService.GetMealOrderByIdAsync(orderId);
        if (order is null || order.PatientId != patientId)
            return Unauthorized(new { Message = "You can only reject your own orders." });

        var updatedOrder = await _mealService.UpdateMealOrderStatusAsync(orderId, "Rejected");
        if (updatedOrder is null)
            return NotFound(new { Message = "Order not found." });

        return Ok(new
        {
            Message = "Meal order rejected successfully.",
            Order = updatedOrder
        });
    }
}