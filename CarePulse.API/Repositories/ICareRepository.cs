using CarePulse.API.Models;

namespace CarePulse.API.Repositories;

public interface ICareRepository
{
    Task<Appointment> AddAppointmentAsync(Appointment appointment);
    Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(int patientId);
    Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId);
    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
    Task<Appointment?> GetAppointmentByIdAsync(int appointmentId);

    Task<DoctorAvailabilitySlot> AddDoctorAvailabilitySlotAsync(DoctorAvailabilitySlot slot);
    Task<IEnumerable<DoctorAvailabilitySlot>> GetDoctorAvailabilitySlotsAsync(int doctorId);
    Task<IEnumerable<DoctorAvailabilitySlot>> GetAvailableDoctorAvailabilitySlotsAsync(int doctorId);
    Task<DoctorAvailabilitySlot?> GetDoctorAvailabilitySlotByIdAsync(int slotId);
    Task<bool> TryBookSlotAsync(int slotId);
    Task<DoctorAvailabilitySlot?> UpdateDoctorAvailabilitySlotAsync(DoctorAvailabilitySlot slot);

    Task<Prescription> AddPrescriptionAsync(Prescription prescription);
    Task<IEnumerable<Prescription>> GetPrescriptionsByPatientAsync(int patientId);

    Task<DietPlan> AddDietPlanAsync(DietPlan dietPlan);
    Task<IEnumerable<DietPlan>> GetDietPlansByPatientAsync(int patientId);
    Task<IEnumerable<DietPlan>> GetAllDietPlansAsync();

    Task<MenuItem> AddMenuItemAsync(MenuItem menuItem);
    Task<IEnumerable<MenuItem>> GetMenuItemsAsync();
    Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync();
    Task<MenuItem?> GetMenuItemByIdAsync(int id);
    Task<MenuItem?> UpdateMenuItemAsync(MenuItem menuItem);
    Task RemoveMenuItemAsync(MenuItem menuItem);

    Task<MealOrder> AddMealOrderAsync(MealOrder order);
    Task<IEnumerable<MealOrder>> GetMealOrdersByPatientAsync(int patientId);
    Task<IEnumerable<MealOrder>> GetAllMealOrdersAsync();
    Task<MealOrder?> GetMealOrderByIdAsync(int id);
    Task<MealOrder?> UpdateMealOrderStatusAsync(int orderId, string status);
    Task<MealOrder?> UpdateMealOrderAsync(MealOrder mealOrder);
    Task RemoveMealOrderAsync(MealOrder mealOrder);
    Task SaveChangesAsync();
}
