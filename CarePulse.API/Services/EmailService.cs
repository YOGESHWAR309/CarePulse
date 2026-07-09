using System.Net;
using System.Net.Mail;

namespace CarePulse.API.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    private (string SmtpHost, int SmtpPort, string SenderEmail, string SenderPassword, bool EnableSsl) GetSmtpSettings()
    {
        var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        var smtpPort = int.TryParse(_configuration["Email:SmtpPort"], out var port) ? port : 587;
        var senderEmail = _configuration["Email:SenderEmail"] ?? "carepulse751@gmail.com";
        var senderPassword = _configuration["Email:SenderPassword"] ?? string.Empty;
        var enableSsl = bool.TryParse(_configuration["Email:EnableSsl"], out var ssl) && ssl;

        return (smtpHost, smtpPort, senderEmail, senderPassword, enableSsl);
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string fullName, string resetToken)
    {
        try
        {
            var (smtpHost, smtpPort, senderEmail, senderPassword, enableSsl) = GetSmtpSettings();
            var appUrl = _configuration["Email:AppUrl"] ?? "http://localhost:3000";

            // Create reset link
            var resetLink = $"{appUrl}/reset-password?token={resetToken}";

            var subject = "CarePulse - Password Reset Request";
            var body = $@"
                <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 4px;'>
                            <h2 style='color: #1976d2;'>Password Reset Request</h2>
                            <p>Hello {fullName},</p>
                            <p>You requested to reset your password. Click the link below to proceed:</p>
                            <p style='margin: 20px 0;'>
                                <a href='{resetLink}' style='background-color: #1976d2; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px; display: inline-block;'>
                                    Reset Password
                                </a>
                            </p>
                            <p>Or copy and paste this token into the password reset form:</p>
                            <div style='background-color: #f5f5f5; padding: 12px; border-radius: 4px; font-family: monospace; word-break: break-all;'>
                                {resetToken}
                            </div>
                            <p style='color: #999; font-size: 12px;'>
                                This link will expire in 1 hour. If you didn't request this, please ignore this email.
                            </p>
                            <hr style='margin: 20px 0; border: none; border-top: 1px solid #ddd;' />
                            <p style='color: #999; font-size: 12px; text-align: center;'>
                                CarePulse Healthcare System<br/>
                                Do not reply to this email.
                            </p>
                        </div>
                    </body>
                </html>
            ";

            if (string.IsNullOrWhiteSpace(senderPassword))
            {
                _logger.LogWarning("SMTP sender password is not configured. Configure Email:SenderPassword or the Email__SenderPassword environment variable to send emails.");
                _logger.LogInformation("[SMTP not configured] Password reset email for {Email}: Token: {Token} Link: {Link}", email, resetToken, resetLink);
                return false;
            }

            // For production: Send actual email via SMTP
            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                var mailMessage = new MailMessage(senderEmail, email)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                await client.SendMailAsync(mailMessage);
            }

            _logger.LogInformation($"Password reset email sent to {email}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending password reset email to {email}");
            return false;
        }
    }

    public async Task<bool> SendPasswordResetOtpEmailAsync(string email, string fullName, string otp)
    {
        try
        {
            var (smtpHost, smtpPort, senderEmail, senderPassword, enableSsl) = GetSmtpSettings();

            var subject = "CarePulse - Password Reset OTP";
            var body = $@"
                <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 4px;'>
                            <h2 style='color: #1976d2;'>Password Reset Request</h2>
                            <p>Hello {fullName},</p>
                            <p>You requested to reset your password. Use the code below to proceed:</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <div style='background-color: #f5f5f5; padding: 20px; border-radius: 4px; display: inline-block;'>
                                    <span style='font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #1976d2; font-family: monospace;'>{otp}</span>
                                </div>
                            </div>
                            <p style='color: #999; font-size: 12px; text-align: center;'>
                                This code will expire in 15 minutes.
                            </p>
                            <p style='color: #d32f2f; font-size: 12px;'>
                                ⚠️ Never share this code with anyone. CarePulse staff will never ask for your OTP.
                            </p>
                            <hr style='margin: 20px 0; border: none; border-top: 1px solid #ddd;' />
                            <p style='color: #999; font-size: 12px; text-align: center;'>
                                CarePulse Healthcare System<br/>
                                Do not reply to this email.
                            </p>
                        </div>
                    </body>
                </html>
            ";

            if (string.IsNullOrWhiteSpace(senderPassword))
            {
                _logger.LogWarning("SMTP sender password is not configured. Configure Email:SenderPassword or the Email__SenderPassword environment variable to send OTP emails.");
                _logger.LogInformation("[SMTP not configured] Password reset OTP for {Email}: {Otp}", email, otp);
                return false;
            }

            // For production: Send actual email via SMTP
            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                var mailMessage = new MailMessage(senderEmail, email)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                await client.SendMailAsync(mailMessage);
            }

            _logger.LogInformation($"Password reset OTP email sent to {email}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending password reset OTP email to {email}");
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string email, string fullName)
    {
        try
        {
            var (smtpHost, smtpPort, senderEmail, senderPassword, enableSsl) = GetSmtpSettings();

            var subject = "Welcome to CarePulse";
            var body = $@"
                <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 4px;'>
                            <h2 style='color: #1976d2;'>Welcome to CarePulse</h2>
                            <p>Hello {fullName},</p>
                            <p>Your account has been created successfully. You can now log in with your email and password.</p>
                            <p style='color: #999; font-size: 12px;'>
                                If you have any issues, please contact our support team.
                            </p>
                            <hr style='margin: 20px 0; border: none; border-top: 1px solid #ddd;' />
                            <p style='color: #999; font-size: 12px; text-align: center;'>
                                CarePulse Healthcare System
                            </p>
                        </div>
                    </body>
                </html>
            ";

            if (string.IsNullOrWhiteSpace(senderPassword))
            {
                _logger.LogWarning("SMTP sender password is not configured. Configure Email:SenderPassword or the Email__SenderPassword environment variable to send welcome emails.");
                _logger.LogInformation("[SMTP not configured] Welcome email for {Email}", email);
                return false;
            }

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = enableSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                var mailMessage = new MailMessage(senderEmail, email)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                await client.SendMailAsync(mailMessage);
            }

            _logger.LogInformation($"Welcome email sent to {email}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending welcome email to {email}");
            return false;
        }
    }

    public async Task<bool> SendEmailAsync(string email, string subject, string body)
    {
        try
        {
            var (smtpHost, smtpPort, senderEmail, senderPassword, enableSsl) = GetSmtpSettings();
            if (string.IsNullOrWhiteSpace(senderPassword))
            {
                _logger.LogWarning("SMTP sender password is not configured. Configure Email:SenderPassword or the Email__SenderPassword environment variable to send emails.");
                _logger.LogInformation("[SMTP not configured] Email to {Email}: Subject: {Subject} Body: {Body}", email, subject, body);
                return false;
            }

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = enableSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                var mailMessage = new MailMessage(senderEmail, email)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                await client.SendMailAsync(mailMessage);
            }

            _logger.LogInformation($"Email sent to {email}: {subject}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending email to {email}");
            return false;
        }
    }
}
