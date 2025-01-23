using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/whatsapp")]
public class WhatsAppController : ControllerBase
{
    private readonly WhatsAppService _whatsAppService;

    public WhatsAppController(WhatsAppService whatsAppService)
    {
        _whatsAppService = whatsAppService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] WhatsAppMessageRequest request)
    {
        if (string.IsNullOrEmpty(request.To) || string.IsNullOrEmpty(request.Message))
        {
            return BadRequest("Recipient and message cannot be empty.");
        }

        try
        {
            await _whatsAppService.SendMessageAsync(request.To, request.Message);
            return Ok("Message sent successfully!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to send message: {ex.Message}");
        }
    }
}

public class WhatsAppMessageRequest
{
    public string? To { get; set; } // 將 To 設為 nullable
    public string? Message { get; set; } // 將 Message 設為 nullable
}