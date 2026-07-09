using CarePulse.API.DTOs;
using CarePulse.API.Models;
using CarePulse.API.Repositories;

namespace CarePulse.API.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly ICareRepository _careRepository;

    public PrescriptionService(ICareRepository careRepository)
    {
        _careRepository = careRepository;
    }

    public async Task<Prescription> CreatePrescriptionAsync(PrescriptionDto dto)
    {
        var prescription = new Prescription
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            Medication = dto.Medication,
            Dosage = dto.Dosage,
            Instructions = dto.Instructions
        };

        await _careRepository.AddPrescriptionAsync(prescription);
        await _careRepository.SaveChangesAsync();
        return prescription;
    }

    public Task<IEnumerable<Prescription>> GetPrescriptionsByPatientAsync(int patientId)
    {
        return _careRepository.GetPrescriptionsByPatientAsync(patientId);
    }
}
