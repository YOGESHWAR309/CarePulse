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
[Authorize(Roles = UserRoles.Doctor)]
public class DoctorController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IPrescriptionService _prescriptionService;

    public DoctorController(IAppointmentService appointmentService, IPrescriptionService prescriptionService)
    {
        _appointmentService = appointmentService;
        _prescriptionService = prescriptionService;
    }

    [HttpGet("schedule")]
    public async Task<IActionResult> GetSchedule()
    {
        var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var appointments = await _appointmentService.GetAppointmentsByDoctorAsync(doctorId);
        return Ok(new
        {
            Message = "Doctor schedule access granted.",
            Schedule = appointments.Select(a => a.ToDto())
        });
    }

    [HttpGet("patients")]
    public IActionResult GetPatients()
    {
        return Ok(new
        {
            Message = "Doctor can view patient list and records.",
            Patients = new[]
            {
                new { PatientId = 1, FullName = "John Doe", Age = 54, Condition = "Diabetes" },
                new { PatientId = 2, FullName = "Jane Roe", Age = 40, Condition = "Hypertension" }
            }
        });
    }

    [HttpPost("prescription")]
    public async Task<IActionResult> CreatePrescription(PrescriptionDto prescription)
    {
        prescription.DoctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var prescriptionModel = await _prescriptionService.CreatePrescriptionAsync(prescription);

        return Ok(new
        {
            Message = "Prescription created successfully.",
            Prescription = prescriptionModel
        });
    }

    [HttpGet("prescriptions")]
    public async Task<IActionResult> GetPrescriptions([FromQuery] int patientId)
    {
        var prescriptions = await _prescriptionService.GetPrescriptionsByPatientAsync(patientId);
        return Ok(new
        {
            Message = "Doctor can view prescriptions for a patient.",
            Prescriptions = prescriptions
        });
    }
}