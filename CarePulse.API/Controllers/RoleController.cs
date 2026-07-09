using CarePulse.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoleController : ControllerBase
{
    [HttpGet("admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public IActionResult AdminOnly()
    {
        return Ok(new { Message = "Admin access granted." });
    }

    [HttpGet("patient")]
    [Authorize(Roles = UserRoles.Patient)]
    public IActionResult PatientOnly()
    {
        return Ok(new { Message = "Patient access granted." });
    }

    [HttpGet("doctor")]
    [Authorize(Roles = UserRoles.Doctor)]
    public IActionResult DoctorOnly()
    {
        return Ok(new { Message = "Doctor access granted." });
    }

    [HttpGet("receptionist")]
    [Authorize(Roles = UserRoles.Receptionist)]
    public IActionResult ReceptionistOnly()
    {
        return Ok(new { Message = "Receptionist access granted." });
    }

    [HttpGet("nutritionist")]
    [Authorize(Roles = UserRoles.Nutritionist)]
    public IActionResult NutritionistOnly()
    {
        return Ok(new { Message = "Nutritionist access granted." });
    }

    [HttpGet("dietician")]
    [Authorize(Roles = UserRoles.Dietician)]
    public IActionResult DieticianOnly()
    {
        return Ok(new { Message = "Dietician access granted." });
    }

    [HttpGet("canteen")]
    [Authorize(Roles = UserRoles.Canteen)]
    public IActionResult CanteenOnly()
    {
        return Ok(new { Message = "Canteen access granted." });
    }
}