namespace CarePulse.API.DTOs;

public class UserDependencyInfoDto
{
    public int AppointmentCount { get; set; }
    public int PrescriptionCount { get; set; }
    public int DietPlanCount { get; set; }
    public int MealOrderCount { get; set; }
    public int RefreshTokenCount { get; set; }
    public int PasswordResetTokenCount { get; set; }
    public int PasswordResetOtpCount { get; set; }
}
