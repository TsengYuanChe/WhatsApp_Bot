using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/whatsapp")]
public class WhatsAppController : ControllerBase
{
    private readonly WhatsAppService _whatsAppService;

    public WhatsAppController(WhatsAppService whatsAppService)
    {
        _whatsAppService = whatsAppService;
    }

    // 接收驗證請求 (Webhook Verification)
    [HttpGet("webhook")]
    public IActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string hubMode,
        [FromQuery(Name = "hub.verify_token")] string hubVerifyToken,
        [FromQuery(Name = "hub.challenge")] string hubChallenge)
    {
        const string verifyToken = "adam880614"; // 替換為你的 Verify Token

        if (hubMode == "subscribe" && hubVerifyToken == verifyToken)
        {
            Console.WriteLine("Webhook verification succeeded.");
            return Ok(hubChallenge); // 返回 hub.challenge
        }

        Console.WriteLine("Webhook verification failed.");
        return Unauthorized("Invalid verify token.");
    }

    // 發送消息
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] WhatsAppMessageRequest request)
    {
        if (string.IsNullOrEmpty(request.To))
        {
            return BadRequest("Recipient (To) cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(request.Message))
        {
            return BadRequest("Message cannot be null or empty.");
        }

        try
        {
            await _whatsAppService.SendMessageAsync(request.To, request.Message);
            return Ok(new { success = true, message = "Message sent successfully!" });
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, "Failed to connect to WhatsApp API.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
        }
    }
}

// 接收消息的模型
public class WhatsAppMessageRequest
{
    [Required(ErrorMessage = "Recipient (To) is required.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Recipient must be a valid phone number.")]
    public string? To { get; set; }

    [Required(ErrorMessage = "Message is required.")]
    [StringLength(4096, ErrorMessage = "Message is too long.")]
    public string? Message { get; set; }
}