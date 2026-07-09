using CarePulse.API.DTOs;
using CarePulse.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DietPlanController : ControllerBase
{
    private readonly IDietPlanService _dietPlanService;

    public DietPlanController(IDietPlanService dietPlanService)
    {
        _dietPlanService = dietPlanService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDietPlan(DietPlanDto dto)
    {
        var createdById = int.TryParse(User.FindFirst("sub")?.Value, out var id) ? id : 0;
        var dietPlan = await _dietPlanService.CreateDietPlanAsync(dto, createdById);
        return Ok(new { Message = "Diet plan created successfully.", DietPlan = dietPlan });
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetDietPlansByPatient(int patientId)
    {
        var dietPlans = await _dietPlanService.GetDietPlansByPatientAsync(patientId);
        return Ok(dietPlans);
    }
}
