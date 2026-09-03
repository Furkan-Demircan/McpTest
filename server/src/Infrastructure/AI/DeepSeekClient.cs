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
            throw new InvalidOperationException("DeepSeek API anahtarı yapılandırılmamış. Lütfen .env dosyasında veya ortam değişkenlerinde 'DEEPSEEK_API_KEY' tanımlayınız.");
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
                    content = "Sen bu kullanıcı yönetim ve kişisel bilgi form sisteminin akıllı asistanısın. Kullanıcılara nazik, yardımsever ve Türkçe olarak yanıt verirsin."
                },
                new
                {
                    role = "user",
                    content = message
                }
            }
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "chat/completions");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                apiKey);

        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"DeepSeek API hatası (Status: {response.StatusCode}): {errorContent}");
        }

        var json =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        using var document =
            JsonDocument.Parse(json);

        return document
            .RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }
}