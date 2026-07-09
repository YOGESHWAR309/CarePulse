using CarePulse.API.DTOs;
using CarePulse.API.Models;
using CarePulse.API.Repositories;

namespace CarePulse.API.Services;

public class DietPlanService : IDietPlanService
{
    private readonly ICareRepository _careRepository;

    public DietPlanService(ICareRepository careRepository)
    {
        _careRepository = careRepository;
    }

    public async Task<DietPlan> CreateDietPlanAsync(DietPlanDto dto, int createdById)
    {
        var plan = new DietPlan
        {
            PatientId = dto.PatientId,
            CreatedById = createdById,
            DietSummary = dto.DietSummary,
            Goal = dto.Goal,
            Notes = dto.Notes
        };

        await _careRepository.AddDietPlanAsync(plan);
        await _careRepository.SaveChangesAsync();
        return plan;
    }

    public Task<IEnumerable<DietPlan>> GetDietPlansByPatientAsync(int patientId)
    {
        return _careRepository.GetDietPlansByPatientAsync(patientId);
    }

    public Task<IEnumerable<DietPlan>> GetAllDietPlansAsync()
    {
        return _careRepository.GetAllDietPlansAsync();
    }
}
