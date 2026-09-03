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
        var apiKey =
            _configuration["DeepSeek:ApiKey"];

        var requestBody = new
        {
            model = "deepseek-chat",
            messages = new[]
            {
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

        response.EnsureSuccessStatusCode();

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

public interface IDeepSeekClient
{
}