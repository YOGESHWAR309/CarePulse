using CarePulse.API.DTOs;
using CarePulse.API.Models;
using CarePulse.API.Repositories;

namespace CarePulse.API.Services;

public class MealService : IMealService
{
    private readonly ICareRepository _careRepository;

    public MealService(ICareRepository careRepository)
    {
        _careRepository = careRepository;
    }

    public async Task<MenuItem> AddMenuItemAsync(MenuItemDto menuItemDto)
    {
        var item = new MenuItem
        {
            Name = menuItemDto.Name,
            Description = menuItemDto.Description,
            Price = menuItemDto.Price,
            FoodCategory = menuItemDto.FoodCategory,
            IsAvailable = menuItemDto.IsAvailable
        };

        await _careRepository.AddMenuItemAsync(item);
        await _careRepository.SaveChangesAsync();
        return item;
    }

    public Task<IEnumerable<MenuItem>> GetMenuItemsAsync()
    {
        return _careRepository.GetMenuItemsAsync();
    }

    public async Task<MenuItem?> UpdateMenuItemAsync(int menuItemId, MenuItemDto menuItemDto)
    {
        var existingMenuItem = await _careRepository.GetMenuItemByIdAsync(menuItemId);
        if (existingMenuItem is null)
            return null;

        existingMenuItem.Name = menuItemDto.Name;
        existingMenuItem.Description = menuItemDto.Description;
        existingMenuItem.Price = menuItemDto.Price;
        existingMenuItem.FoodCategory = menuItemDto.FoodCategory;
        existingMenuItem.IsAvailable = menuItemDto.IsAvailable;

        var updatedMenuItem = await _careRepository.UpdateMenuItemAsync(existingMenuItem);
        await _careRepository.SaveChangesAsync();
        return updatedMenuItem;
    }

    public async Task<bool> DeleteMenuItemAsync(int menuItemId)
    {
        var existingMenuItem = await _careRepository.GetMenuItemByIdAsync(menuItemId);
        if (existingMenuItem is null)
            return false;

        await _careRepository.RemoveMenuItemAsync(existingMenuItem);
        await _careRepository.SaveChangesAsync();
        return true;
    }

    public async Task<MealOrder> PlaceMealOrderAsync(MealOrderRequestDto request)
    {
        var order = new MealOrder
        {
            PatientId = request.PatientId,
            MenuItemId = request.MenuItemId,
            Quantity = request.Quantity,
            Status = "Pending"
        };

        await _careRepository.AddMealOrderAsync(order);
        await _careRepository.SaveChangesAsync();
        return order;
    }

    public Task<IEnumerable<MealOrder>> GetMealOrdersByPatientAsync(int patientId)
    {
        return _careRepository.GetMealOrdersByPatientAsync(patientId);
    }

    public Task<IEnumerable<MealOrder>> GetAllMealOrdersAsync()
    {
        return _careRepository.GetAllMealOrdersAsync();
    }

    public Task<MealOrder?> GetMealOrderByIdAsync(int orderId)
    {
        return _careRepository.GetMealOrderByIdAsync(orderId);
    }

    public async Task<MealOrder?> UpdateMealOrderStatusAsync(int orderId, string status)
    {
        var order = await _careRepository.UpdateMealOrderStatusAsync(orderId, status);
        if (order is null)
            return null;

        await _careRepository.SaveChangesAsync();
        return order;
    }
}
