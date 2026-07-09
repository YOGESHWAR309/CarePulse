using System.Security.Claims;
using CarePulse.API.DTOs;
using CarePulse.API.Helpers;
using CarePulse.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoles.Dietician)]
public class DieticianController : ControllerBase
{
    private readonly IDietPlanService _dietPlanService;

    public DieticianController(IDietPlanService dietPlanService)
    {
        _dietPlanService = dietPlanService;
    }

    [HttpGet("diet-plans")]
    public IActionResult GetDietPlans()
    {
        // Return diet plans from database for richer variety
        var plans = _dietPlanService.GetAllDietPlansAsync().Result;
        var payload = plans.Select(p => new
        {
            PlanId = p.DietPlanId,
            Patient = p.Patient?.FullName ?? $"ID {p.PatientId}",
            CreatedBy = p.CreatedBy?.FullName ?? $"ID {p.CreatedById}",
            Summary = p.DietSummary,
            Goal = p.Goal,
            Notes = p.Notes,
            CreatedAt = p.CreatedAt
        });

        return Ok(new
        {
            Message = "Dietician can view patient diet plans.",
            DietPlans = payload
        });
    }

    [HttpPost("diet-plans")]
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

    [HttpGet("nutrition")]
    public IActionResult GetNutritionGuidance()
    {
        return Ok(new
        {
            Message = "Dietician can view nutrition-specific guidance.",
            Guidance = new[]
            {
                "Increase fiber intake",
                "Include lean protein at each meal",
                "Avoid processed sugars"
            }
        });
    }
}