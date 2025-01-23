using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

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
    [HttpPost("webhook")]
    public IActionResult ReceiveMessage([FromBody] dynamic request)
    {
        Console.WriteLine($"Incoming Webhook: {JsonConvert.SerializeObject(request, Formatting.Indented)}");

        // 解析收到的消息
        var sender = request.entry[0]?.changes[0]?.value?.messages?[0]?.from;
        var messageText = request.entry[0]?.changes[0]?.value?.messages?[0]?.text?.body;

        if (!string.IsNullOrEmpty(sender) && !string.IsNullOrEmpty(messageText))
        {
            Console.WriteLine($"Received message from {sender}: {messageText}");

            // 自動回覆
            _whatsAppService.SendMessageAsync(sender, $"You said: {messageText}").Wait();
        }

        return Ok();
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