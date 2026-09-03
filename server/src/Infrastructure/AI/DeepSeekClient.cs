using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.AI;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.AI;

public class DeepSeekClient : IDeepSeekClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public DeepSeekClient(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> ChatAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["DeepSeek:ApiKey"]
            ?? _configuration["DEEPSEEK_API_KEY"];

        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "your_deepseek_api_key_here")
        {
            throw new InvalidOperationException("DeepSeek API anahtarı yapılandırılmamış. Lütfen kök dizindeki .env dosyasında 'DEEPSEEK_API_KEY' alanına gerçek API anahtarınızı giriniz.");
        }

        var model = _configuration["DeepSeek:Model"] ?? "deepseek-chat";

        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = "Sen kullanıcı yönetim ve kişisel bilgi formu sisteminin yapay zeka asistanısın. Kullanıcılara nazik, yardımcı, kısa ve net Türkçe yanıtlar ver. Formdaki alanlar: Ad, Soyad, 11 haneli TC Kimlik Numarası, E-posta, Anne Adı, Baba Adı ve Doğum Tarihi'dir. Veritabanı olarak PostgreSQL kullanılmaktadır."
                },
                new
                {
                    role = "user",
                    content = message
                }
            },
            stream = false,
            temperature = 0.7
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "chat/completions");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                apiKey.Trim());

        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"DeepSeek API hatası (HTTP {(int)response.StatusCode}): {json}");
        }

        using var document = JsonDocument.Parse(json);

        if (document.RootElement.TryGetProperty("choices", out var choices) &&
            choices.GetArrayLength() > 0 &&
            choices[0].TryGetProperty("message", out var messageElement) &&
            messageElement.TryGetProperty("content", out var contentElement))
        {
            return contentElement.GetString() ?? string.Empty;
        }

        return "DeepSeek modelinden geçerli bir yanıt alınamadı.";
    }
}