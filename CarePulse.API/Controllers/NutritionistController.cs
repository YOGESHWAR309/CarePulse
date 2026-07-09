using System.Security.Claims;
using CarePulse.API.DTOs;
using CarePulse.API.Helpers;
using CarePulse.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoles.Nutritionist)]
public class NutritionistController : ControllerBase
{
    private readonly IDietPlanService _dietPlanService;

    public NutritionistController(IDietPlanService dietPlanService)
    {
        _dietPlanService = dietPlanService;
    }

    [HttpGet("plans")]
    public IActionResult GetDietPlans()
    {
        return Ok(new
        {
            Message = "Nutritionist can view diet plans.",
            DietPlans = new[]
            {
                new { PlanId = 1, Patient = "John Doe", Goal = "Weight Loss", Status = "Active" },
                new { PlanId = 2, Patient = "Jane Roe", Goal = "Blood Sugar Control", Status = "Active" }
            }
        });
    }

    [HttpPost("plans")]
    public async Task<IActionResult> CreateDietPlan(DietPlanDto dietPlan)
    {
        var createdById = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var plan = await _dietPlanService.CreateDietPlanAsync(dietPlan, createdById);

        return Ok(new
        {
            Message = "Diet plan created successfully.",
            DietPlan = plan
        });
    }

    [HttpGet("plans/patient")]
    public async Task<IActionResult> GetDietPlansByPatient([FromQuery] int patientId)
    {
        var dietPlans = await _dietPlanService.GetDietPlansByPatientAsync(patientId);
        return Ok(new
        {
            Message = "Nutritionist can view diet plans for a patient.",
            DietPlans = dietPlans
        });
    }

    [HttpGet("patients")]
    public IActionResult GetNutritionPatients()
    {
        return Ok(new
        {
            Message = "Nutritionist can manage patient nutrition.",
            Patients = new[]
            {
                new { PatientId = 1, Name = "John Doe", Status = "Dietary Review" },
                new { PatientId = 2, Name = "Jane Roe", Status = "Meal Plan Assigned" }
            }
        });
    }
}