using CarePulse.API.DTOs;
using CarePulse.API.Helpers;
using CarePulse.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoles.Canteen)]
public class CanteenController : ControllerBase
{
    private readonly IMealService _mealService;

    public CanteenController(IMealService mealService)
    {
        _mealService = mealService;
    }

    [HttpGet("menu")]
    public async Task<IActionResult> GetMenu()
    {
        var menuItems = await _mealService.GetMenuItemsAsync();
        return Ok(new
        {
            Message = "Canteen manager can view meal menu.",
            MenuItems = menuItems
        });
    }

    [HttpPost("menu")]
    public async Task<IActionResult> AddMenuItem(MenuItemDto menuItemDto)
    {
        var item = await _mealService.AddMenuItemAsync(menuItemDto);

        return Ok(new
        {
            Message = "Menu item created successfully.",
            MenuItem = item
        });
    }

    [HttpPut("menu/{id}")]
    public async Task<IActionResult> UpdateMenuItem(int id, MenuItemDto menuItemDto)
    {
        var item = await _mealService.UpdateMenuItemAsync(id, menuItemDto);
        if (item is null)
            return NotFound(new { Message = "Menu item not found." });

        return Ok(new
        {
            Message = "Menu item updated successfully.",
            MenuItem = item
        });
    }

    [HttpDelete("menu/{id}")]
    public async Task<IActionResult> DeleteMenuItem(int id)
    {
        var deleted = await _mealService.DeleteMenuItemAsync(id);
        if (!deleted)
            return NotFound(new { Message = "Menu item not found." });

        return Ok(new
        {
            Message = "Menu item deleted successfully.",
            MenuItemId = id
        });
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders([FromQuery] int? patientId)
    {
        var orders = patientId.HasValue
            ? await _mealService.GetMealOrdersByPatientAsync(patientId.Value)
            : await _mealService.GetAllMealOrdersAsync();

        return Ok(new
        {
            Message = patientId.HasValue
                ? "Canteen manager can view orders for a patient."
                : "Canteen manager can view all patient orders.",
            Orders = orders
        });
    }

    [HttpPost("orders")]
    public async Task<IActionResult> ManageOrders(OrderUpdateDto order)
    {
        var mealOrder = await _mealService.UpdateMealOrderStatusAsync(order.OrderId, order.Status);
        if (mealOrder is null)
            return NotFound(new { Message = "Order not found." });

        return Ok(new
        {
            Message = "Canteen manager updated order status.",
            Order = mealOrder
        });
    }
}