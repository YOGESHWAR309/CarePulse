using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CarePulse.API.Services;

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;
    private readonly IConfiguration _configuration;

    public SmsService(ILogger<SmsService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            _logger.LogWarning("Cannot send SMS: phone number is empty.");
            return false;
        }

        // Basic E.164-ish validation: allow optional leading + and 7-15 digits.
        // Normalize: remove spaces, parentheses, dashes, etc. but keep leading +.
        var normalized = System.Text.RegularExpressions.Regex.Replace(phoneNumber.Trim(), "[^\\d+]", "");

        if (normalized.StartsWith("00", StringComparison.Ordinal))
        {
            normalized = "+" + normalized[2..];
        }

        if (normalized.StartsWith("+", StringComparison.Ordinal))
        {
            // Already international format.
        }
        else if (normalized.StartsWith("0", StringComparison.Ordinal) && normalized.Length == 11)
        {
            normalized = "+91" + normalized[1..];
        }
        else if (normalized.StartsWith("91", StringComparison.Ordinal) && normalized.Length == 12)
        {
            normalized = "+" + normalized;
        }
        else if (normalized.StartsWith("091", StringComparison.Ordinal) && normalized.Length == 13)
        {
            normalized = "+91" + normalized[3..];
        }
        else if (System.Text.RegularExpressions.Regex.IsMatch(normalized, "^\\d{10}$"))
        {
            normalized = "+91" + normalized;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, "^\\+\\d{10,15}$"))
        {
            _logger.LogWarning("SMS not sent - invalid phone number format after normalization: {PhoneNumber} -> {Normalized}", phoneNumber, normalized);
            return false;
        }

        var configuredSid = _configuration["Sms:TwilioAccountSid"];
        var configuredToken = _configuration["Sms:TwilioAuthToken"];
        var configuredFrom = _configuration["Sms:FromNumber"];
        var envSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
        var envToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
        var envFrom = Environment.GetEnvironmentVariable("TWILIO_FROM");

        var twilioSid = SelectEffectiveValue(configuredSid, envSid);
        var twilioToken = SelectEffectiveValue(configuredToken, envToken);
        var twilioFrom = SelectEffectiveValue(configuredFrom, envFrom);

        if (IsMissingTwilioValue(twilioSid) || IsMissingTwilioValue(twilioToken) || IsMissingTwilioValue(twilioFrom))
        {
            _logger.LogWarning(
                "Twilio SMS is not configured or is using placeholder credentials. " +
                "Set Sms:TwilioAccountSid, Sms:TwilioAuthToken, Sms:FromNumber in configuration or set TWILIO_ACCOUNT_SID, TWILIO_AUTH_TOKEN, TWILIO_FROM as environment variables. " +
                "Effective sources: Sid={SidSource}, Token={TokenSource}, From={FromSource}.",
                CredentialSource(configuredSid, envSid),
                CredentialSource(configuredToken, envToken),
                CredentialSource(configuredFrom, envFrom));
            _logger.LogInformation("[SMS fallback] To: {PhoneNumber} Message: {Message}", phoneNumber, message);
            return true;
        }

        static string? SelectEffectiveValue(string? configuredValue, string? envValue)
            => !IsMissingTwilioValue(configuredValue) ? configuredValue : envValue;

        static bool IsMissingTwilioValue(string? value)
            => string.IsNullOrWhiteSpace(value)
                || value.Contains("YOUR_TWILIO", StringComparison.OrdinalIgnoreCase)
                || value.Contains("PLACEHOLDER", StringComparison.OrdinalIgnoreCase);

        static string CredentialSource(string? configuredValue, string? envValue)
            => !IsMissingTwilioValue(configuredValue) ? "config"
                : !string.IsNullOrWhiteSpace(envValue) ? "environment"
                : "missing";

        try
        {
            using var client = new HttpClient();
            var requestUri = $"https://api.twilio.com/2010-04-01/Accounts/{twilioSid}/Messages.json";
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("To", normalized),
                new KeyValuePair<string, string>("From", twilioFrom),
                new KeyValuePair<string, string>("Body", message)
            });

            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{twilioSid}:{twilioToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

            var resp = await client.PostAsync(requestUri, content);
            if (resp.IsSuccessStatusCode)
            {
                _logger.LogInformation("SMS sent to {PhoneNumber}", phoneNumber);
                return true;
            }

            var respBody = await resp.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to send SMS to {PhoneNumber}. Status: {Status} Response: {Response}", phoneNumber, resp.StatusCode, respBody);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }
}
