using CarePulse.API.DTOs;
using CarePulse.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MealController : ControllerBase
{
    private readonly IMealService _mealService;

    public MealController(IMealService mealService)
    {
        _mealService = mealService;
    }

    [HttpGet("menu")]
    public async Task<IActionResult> GetMenu()
    {
        var menuItems = await _mealService.GetMenuItemsAsync();
        return Ok(menuItems);
    }

    [HttpPost("menu")]
    public async Task<IActionResult> AddMenuItem(MenuItemDto dto)
    {
        var menuItem = await _mealService.AddMenuItemAsync(dto);
        return Ok(new { Message = "Menu item created successfully.", MenuItem = menuItem });
    }

    [HttpPost("orders")]
    public async Task<IActionResult> PlaceMealOrder(MealOrderRequestDto request)
    {
        var order = await _mealService.PlaceMealOrderAsync(request);
        return Ok(new { Message = "Meal order placed successfully.", Order = order });
    }

    [HttpGet("orders/patient/{patientId}")]
    public async Task<IActionResult> GetMealOrdersByPatient(int patientId)
    {
        var orders = await _mealService.GetMealOrdersByPatientAsync(patientId);
        return Ok(orders);
    }

    [HttpPut("orders/{orderId}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] OrderUpdateDto dto)
    {
        var order = await _mealService.UpdateMealOrderStatusAsync(orderId, dto.Status);
        if (order is null)
            return NotFound(new { Message = "Order not found." });

        return Ok(order);
    }
}
