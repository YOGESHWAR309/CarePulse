using System.Text;
using System.Text.Json;
using CarePulse.API.DTOs;
using CarePulse.API.Helpers;
using Microsoft.Extensions.Options;

namespace CarePulse.API.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(
        HttpClient httpClient,
        IOptions<GeminiSettings> options,
        ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    private const string SystemPrompt = @"
You are CarePulse AI Assistant.

You are integrated into a Hospital Management System.

Responsibilities:

• Explain diseases in simple language.

• Explain hospital workflows.

• Explain appointments.

• Explain prescriptions.

• Explain laboratory tests.

• Explain medicines in general.

Rules:

• Never diagnose diseases.

• Never prescribe medicines.

• Never change medicines.

• Recommend consulting a doctor.

• Reply politely.

• Maximum 150 words.

• Use bullet points whenever possible.

• Keep answers easy to understand.
";

    public async Task<string> GetChatReplyAsync(ChatRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            return "Gemini API Key is missing.";

        var contents = new List<object>();

        foreach (var history in request.History.TakeLast(10))
        {
            contents.Add(new
            {
                role = history.Role == "model" ? "model" : "user",
                parts = new[]
                {
                    new
                    {
                        text = history.Text
                    }
                }
            });
        }

        contents.Add(new
        {
            role = "user",
            parts = new[]
            {
                new
                {
                    text = request.Message
                }
            }
        });

        var payload = new
        {
            system_instruction = new
            {
                parts = new[]
                {
                    new
                    {
                        text = SystemPrompt
                    }
                }
            },

            contents
        };

        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";

        var json = JsonSerializer.Serialize(payload);

        var httpContent = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        try
        {
            var response =
                await _httpClient.PostAsync(url, httpContent);

            var body =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Gemini API Error ({StatusCode}) : {Body}",
                    response.StatusCode,
                    body);

                try
                {
                    using var errorDoc = JsonDocument.Parse(body);

                    if (errorDoc.RootElement.TryGetProperty("error", out var error))
                    {
                        if (error.TryGetProperty("message", out var message))
                        {
                            return message.GetString()
                                   ?? "Unable to contact Gemini AI.";
                        }
                    }
                }
                catch
                {
                    // Ignore parsing errors
                }

                return "Unable to contact Gemini AI.";
            }

            using JsonDocument document =
                JsonDocument.Parse(body);

            if (!document.RootElement.TryGetProperty(
                    "candidates",
                    out JsonElement candidates))
            {
                return "Gemini returned an invalid response.";
            }

            if (candidates.GetArrayLength() == 0)
            {
                return "Gemini returned an empty response.";
            }

            var candidate = candidates[0];

            if (!candidate.TryGetProperty(
                    "content",
                    out JsonElement content))
            {
                return "Gemini response is missing content.";
            }

            if (!content.TryGetProperty(
                    "parts",
                    out JsonElement parts))
            {
                return "Gemini response contains no parts.";
            }

            if (parts.GetArrayLength() == 0)
            {
                return "Gemini returned no text.";
            }

            var reply =
                parts[0]
                .GetProperty("text")
                .GetString();

            return string.IsNullOrWhiteSpace(reply)
                ? "Gemini generated an empty reply."
                : reply;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini Exception");

            return "An unexpected error occurred while contacting Gemini AI.";
        }
    }
}