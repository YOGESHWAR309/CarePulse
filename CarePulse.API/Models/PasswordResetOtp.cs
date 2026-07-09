using System.ComponentModel.DataAnnotations;

namespace CarePulse.API.Models;

public class PasswordResetOtp
{
    [Key]
    public int OtpId { get; set; }
    public int UserId { get; set; }
    public string Otp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
