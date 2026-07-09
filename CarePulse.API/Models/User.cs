using System.Text.Json.Serialization;

namespace CarePulse.API.Models;

public class User
{
    public int UserId { get; set; }

    public string FullName { get; set; }
        = string.Empty;

    public string Email { get; set; }
        = string.Empty;

    [JsonIgnore]
    public string PasswordHash { get; set; }
        = string.Empty;

    public string Phone { get; set; }
        = string.Empty;

    public string Role { get; set; }
        = "Patient";

    public string? DoctorCategory { get; set; }

    public string? DoctorQualification { get; set; }

    public string? DoctorSpecialty { get; set; }

    public bool IsActive { get; set; }
        = true;

    public DateTime CreatedAt
    {
        get;
        set;
    } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<RefreshToken>
        RefreshTokens
    {
        get;
        set;
    } = new List<RefreshToken>();

    [JsonIgnore]
    public ICollection<PasswordResetOtp>
        PasswordResetOtps
    {
        get;
        set;
    } = new List<PasswordResetOtp>();
}