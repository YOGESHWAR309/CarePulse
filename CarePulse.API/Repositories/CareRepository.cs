using CarePulse.API.Data;
using CarePulse.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CarePulse.API.Repositories;

public class CareRepository : ICareRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CareRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Appointment> AddAppointmentAsync(Appointment appointment)
    {
        await _dbContext.Appointments.AddAsync(appointment);
        return appointment;
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(int patientId)
    {
        return await _dbContext.Appointments
            .Where(x => x.PatientId == patientId)
            .Include(x => x.Doctor)
            .Include(x => x.Patient)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId)
    {
        return await _dbContext.Appointments
            .Where(x => x.DoctorId == doctorId)
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
    {
        return await _dbContext.Appointments
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .ToListAsync();
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId)
    {
        return await _dbContext.Appointments
            .Include(x => x.Doctor)
            .Include(x => x.Patient)
            .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);
    }

    public async Task<bool> TryBookSlotAsync(int slotId)
    {
        var result = await _dbContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE DoctorAvailabilitySlots SET IsBooked = 1 WHERE DoctorAvailabilitySlotId = {slotId} AND IsBooked = 0");
        return result == 1;
    }

    public async Task<DoctorAvailabilitySlot> AddDoctorAvailabilitySlotAsync(DoctorAvailabilitySlot slot)
    {
        await _dbContext.DoctorAvailabilitySlots.AddAsync(slot);
        return slot;
    }

    public async Task<IEnumerable<DoctorAvailabilitySlot>> GetDoctorAvailabilitySlotsAsync(int doctorId)
    {
        return await _dbContext.DoctorAvailabilitySlots
            .Where(x => x.DoctorId == doctorId)
            .OrderBy(x => x.SlotStart)
            .ToListAsync();
    }

    public async Task<IEnumerable<DoctorAvailabilitySlot>> GetAvailableDoctorAvailabilitySlotsAsync(int doctorId)
    {
        return await _dbContext.DoctorAvailabilitySlots
            .Where(x => x.DoctorId == doctorId && !x.IsBooked && x.SlotStart > DateTime.UtcNow)
            .OrderBy(x => x.SlotStart)
            .ToListAsync();
    }

    public async Task<DoctorAvailabilitySlot?> GetDoctorAvailabilitySlotByIdAsync(int slotId)
    {
        return await _dbContext.DoctorAvailabilitySlots.FindAsync(slotId);
    }

    public async Task<DoctorAvailabilitySlot?> UpdateDoctorAvailabilitySlotAsync(DoctorAvailabilitySlot slot)
    {
        var existing = await _dbContext.DoctorAvailabilitySlots.FindAsync(slot.DoctorAvailabilitySlotId);
        if (existing is null)
            return null;

        existing.SlotStart = slot.SlotStart;
        existing.SlotEnd = slot.SlotEnd;
        existing.IsBooked = slot.IsBooked;

        return existing;
    }

    public async Task<Prescription> AddPrescriptionAsync(Prescription prescription)
    {
        await _dbContext.Prescriptions.AddAsync(prescription);
        return prescription;
    }

    public async Task<IEnumerable<Prescription>> GetPrescriptionsByPatientAsync(int patientId)
    {
        return await _dbContext.Prescriptions
            .Where(x => x.PatientId == patientId)
            .ToListAsync();
    }

    public async Task<DietPlan> AddDietPlanAsync(DietPlan dietPlan)
    {
        await _dbContext.DietPlans.AddAsync(dietPlan);
        return dietPlan;
    }

    public async Task<IEnumerable<DietPlan>> GetDietPlansByPatientAsync(int patientId)
    {
        return await _dbContext.DietPlans
            .Where(x => x.PatientId == patientId)
            .ToListAsync();
    }

    public async Task<IEnumerable<DietPlan>> GetAllDietPlansAsync()
    {
        return await _dbContext.DietPlans
            .Include(x => x.Patient)
            .Include(x => x.CreatedBy)
            .ToListAsync();
    }

    public async Task<MenuItem> AddMenuItemAsync(MenuItem menuItem)
    {
        await _dbContext.MenuItems.AddAsync(menuItem);
        return menuItem;
    }

    public async Task<IEnumerable<MenuItem>> GetMenuItemsAsync()
    {
        return await _dbContext.MenuItems.ToListAsync();
    }

    public async Task<MealOrder> AddMealOrderAsync(MealOrder order)
    {
        await _dbContext.MealOrders.AddAsync(order);
        return order;
    }

    public async Task<IEnumerable<MealOrder>> GetMealOrdersByPatientAsync(int patientId)
    {
        return await _dbContext.MealOrders
            .Where(x => x.PatientId == patientId)
            .Include(x => x.MenuItem)
            .Include(x => x.Patient)
            .ToListAsync();
    }

    public async Task<MealOrder?> UpdateMealOrderStatusAsync(int orderId, string status)
    {
        var order = await _dbContext.MealOrders.FindAsync(orderId);
        if (order is null)
            return null;

        order.Status = status;
        return order;
    }

    public async Task<MenuItem?> GetMenuItemByIdAsync(int id)
    {
        return await _dbContext.MenuItems.FindAsync(id);
    }

    public async Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync()
    {
        return await _dbContext.MenuItems.ToListAsync();
    }

    public async Task<MenuItem?> UpdateMenuItemAsync(MenuItem menuItem)
    {
        var existing = await _dbContext.MenuItems.FindAsync(menuItem.MenuItemId);
        if (existing is null)
            return null;

        existing.Name = menuItem.Name;
        existing.Description = menuItem.Description;
        existing.Price = menuItem.Price;
        existing.IsAvailable = menuItem.IsAvailable;
        return existing;
    }

    public Task RemoveMenuItemAsync(MenuItem menuItem)
    {
        _dbContext.MenuItems.Remove(menuItem);
        return Task.CompletedTask;
    }

    public async Task<MealOrder?> GetMealOrderByIdAsync(int id)
    {
        return await _dbContext.MealOrders.FindAsync(id);
    }

    public async Task<IEnumerable<MealOrder>> GetAllMealOrdersAsync()
    {
        return await _dbContext.MealOrders.ToListAsync();
    }

    public async Task<MealOrder?> UpdateMealOrderAsync(MealOrder mealOrder)
    {
        var existing = await _dbContext.MealOrders.FindAsync(mealOrder.MealOrderId);
        if (existing is null)
            return null;

        existing.PatientId = mealOrder.PatientId;
        existing.MenuItemId = mealOrder.MenuItemId;
        existing.Quantity = mealOrder.Quantity;
        existing.Status = mealOrder.Status;
        return existing;
    }

    public Task RemoveMealOrderAsync(MealOrder mealOrder)
    {
        _dbContext.MealOrders.Remove(mealOrder);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
