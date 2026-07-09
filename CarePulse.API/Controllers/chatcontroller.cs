using CarePulse.API.DTOs;
using CarePulse.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IGeminiService _geminiService;

    public ChatController(IGeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] ChatRequestDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new
            {
                message = "Request body is missing."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.Message))
        {
            return BadRequest(new
            {
                message = "Message cannot be empty."
            });
        }

        var reply = await _geminiService.GetChatReplyAsync(dto);

        return Ok(new ChatResponseDto
        {
            Reply = reply
        });
    }
}