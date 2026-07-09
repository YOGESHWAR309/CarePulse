using CarePulse.API.DTOs;
using CarePulse.API.Models;

namespace CarePulse.API.Services;

public interface IMealService
{
    Task<MenuItem> AddMenuItemAsync(MenuItemDto menuItemDto);
    Task<MenuItem?> UpdateMenuItemAsync(int menuItemId, MenuItemDto menuItemDto);
    Task<bool> DeleteMenuItemAsync(int menuItemId);
    Task<IEnumerable<MenuItem>> GetMenuItemsAsync();
    Task<MealOrder> PlaceMealOrderAsync(MealOrderRequestDto request);
    Task<IEnumerable<MealOrder>> GetMealOrdersByPatientAsync(int patientId);
    Task<IEnumerable<MealOrder>> GetAllMealOrdersAsync();
    Task<MealOrder?> GetMealOrderByIdAsync(int orderId);
    Task<MealOrder?> UpdateMealOrderStatusAsync(int orderId, string status);
}
