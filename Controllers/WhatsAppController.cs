using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Text.Json;

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
    public async Task<IActionResult> ReceiveMessage([FromBody] JsonElement request)
    {
        try
        {
            Console.WriteLine($"Incoming Webhook: {request}");

            // 確保 JSON 結構正確
            if (request.TryGetProperty("entry", out var entry))
            {
                foreach (var change in entry[0].GetProperty("changes").EnumerateArray())
                {
                    var value = change.GetProperty("value");

                    // 處理消息
                    if (value.TryGetProperty("messages", out var messages))
                    {
                        foreach (var message in messages.EnumerateArray())
                        {
                            var from = message.GetProperty("from").GetString(); // 發送者的 WhatsApp 編號
                            var text = message.GetProperty("text").GetProperty("body").GetString(); // 訊息內容

                            Console.WriteLine($"Message received from {from}: {text}");

                            // 自動回覆訊息
                            var reply = $"收到您的訊息：{text}";
                            await _whatsAppService.SendMessageAsync(from, reply); // 使用你的 SendMessageAsync 方法
                            Console.WriteLine($"Reply sent to {from}: {reply}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No 'entry' found in the request.");
                return BadRequest("Invalid Webhook structure: No 'entry' found.");
            }

            return Ok("Webhook processed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing Webhook: {ex.Message}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
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