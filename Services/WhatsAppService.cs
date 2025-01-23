using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

public class WhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly string _accessToken;
    private readonly string _phoneNumberId;

    public WhatsAppService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        _accessToken = configuration["WhatsApp:AccessToken"]
            ?? throw new ArgumentNullException(nameof(_accessToken), "AccessToken 未配置於 appsettings.json 或環境變數中");
        _phoneNumberId = configuration["WhatsApp:PhoneNumberId"]
            ?? throw new ArgumentNullException(nameof(_phoneNumberId), "PhoneNumberId 未配置於 appsettings.json 或環境變數中");
    }

    public async Task SendMessageAsync(string to, string message)
    {
        var url = $"https://graph.facebook.com/v17.0/{_phoneNumberId}/messages";
        var payload = new
        {
            messaging_product = "whatsapp",
            to = to,
            type = "text",
            text = new { body = message }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
        request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        Console.WriteLine("Message sent successfully!");
    }
}