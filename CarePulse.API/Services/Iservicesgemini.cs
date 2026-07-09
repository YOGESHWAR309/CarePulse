using CarePulse.API.DTOs;

namespace CarePulse.API.Services;

public interface IGeminiService
{
    Task<string> GetChatReplyAsync(ChatRequestDto request);
}