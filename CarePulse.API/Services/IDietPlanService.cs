using CarePulse.API.DTOs;
using CarePulse.API.Models;

namespace CarePulse.API.Services;

public interface IDietPlanService
{
    Task<DietPlan> CreateDietPlanAsync(DietPlanDto dto, int createdById);
    Task<IEnumerable<DietPlan>> GetDietPlansByPatientAsync(int patientId);
    Task<IEnumerable<DietPlan>> GetAllDietPlansAsync();
}
