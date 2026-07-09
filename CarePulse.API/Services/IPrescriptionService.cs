using CarePulse.API.DTOs;
using CarePulse.API.Models;

namespace CarePulse.API.Services;

public interface IPrescriptionService
{
    Task<Prescription> CreatePrescriptionAsync(PrescriptionDto dto);
    Task<IEnumerable<Prescription>> GetPrescriptionsByPatientAsync(int patientId);
}
