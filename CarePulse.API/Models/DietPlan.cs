namespace CarePulse.API.Models;

public class DietPlan
{
    public int DietPlanId { get; set; }
    public int PatientId { get; set; }
    public int CreatedById { get; set; }
    public string DietSummary { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? Patient { get; set; }
    public User? CreatedBy { get; set; }
}
