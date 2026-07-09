namespace CarePulse.API.DTOs;

public class ChatMessageDto
{
    public string Role { get; set; } = string.Empty; // "user" or "model"
    public string Text { get; set; } = string.Empty;
}

public class ChatRequestDto
{
    public string Message { get; set; } = string.Empty;
    public List<ChatMessageDto> History { get; set; } = new();
}

public class ChatResponseDto
{
    public string Reply { get; set; } = string.Empty;
}