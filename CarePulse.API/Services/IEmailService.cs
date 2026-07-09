namespace CarePulse.API.Services;

public interface IEmailService
{
    Task<bool> SendPasswordResetEmailAsync(string email, string fullName, string resetToken);
    Task<bool> SendPasswordResetOtpEmailAsync(string email, string fullName, string otp);
    Task<bool> SendWelcomeEmailAsync(string email, string fullName);
    Task<bool> SendEmailAsync(string email, string subject, string body);
}
