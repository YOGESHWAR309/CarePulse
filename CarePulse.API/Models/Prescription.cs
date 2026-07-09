namespace CarePulse.API.Models;

public class Prescription
{
    public int PrescriptionId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public string Medication { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? Patient { get; set; }
    public User? Doctor { get; set; }
}
