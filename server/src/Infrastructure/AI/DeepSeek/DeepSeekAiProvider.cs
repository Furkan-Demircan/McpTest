using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using Application.AI;
using Infrastructure.AI.DeepSeek.Models;

using Microsoft.Extensions.Configuration;

namespace Infrastructure.AI.DeepSeek;

public class DeepSeekAiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public DeepSeekAiProvider(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<AiResponse> ChatAsync(
        AiRequest request,
        CancellationToken cancellationToken = default)
    {
        var apiKey =
            _configuration["DeepSeek:ApiKey"]
            ?? _configuration["DEEPSEEK_API_KEY"];

        if (string.IsNullOrWhiteSpace(apiKey) ||
            apiKey == "your_deepseek_api_key_here")
        {
            throw new InvalidOperationException(
                "DeepSeek API anahtarı yapılandırılmamış.");
        }

        var model =
            _configuration["DeepSeek:Model"]
            ?? "deepseek-v4-flash";

        var currentFormData =
            JsonSerializer.Serialize(
                request.FormData,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        var deepSeekMessages = new List<object>
        {
            new
            {
                role = "system",
                content = """
                Sen, kullanıcı yönetim ve kişisel bilgi formu sisteminde
                çalışan bir yapay zeka asistanısın.

                Görevin:
                - Kullanıcının uygulama hakkındaki sorularını cevaplamak.
                - Uygulamadaki işlemlerin nasıl yapılacağını açıklamak.
                - Kullanıcıyı gerektiğinde doğru sayfaya yönlendirmek.
                - Form doldurma konusunda yardımcı olmak.

                Uygulamadaki sayfalar:
                - Ana Sayfa: /
                - Öğrenci Ekleme Formu: /form (veya /student, /ogrenci)
                - Öğretmen Ekleme Formu: /teacher (veya /ogretmen)
                - Kayıtlı Kullanıcılar: /users

                Önemli sınırlar:
                - Veritabanına doğrudan erişemezsin.
                - Kullanıcı ekleyemezsin.
                - Kullanıcı silemezsin.
                - Kullanıcı bilgilerini güncelleyemezsin.
                - CRUD işlemlerini kendin gerçekleştiremezsin.
                - Gerçekleştirmediğin bir işlemi gerçekleştirmiş gibi söyleme.
                - Bilmediğin bilgileri uydurma.

                Boş string değerleri doldurulmamış kabul et.

                Form doldurma / güncelleme kuralları:
                - Kullanıcı herhangi bir form bilgisi verdiğinde (öğrenci, öğretmen, ders, kayıt veya sayfadaki herhangi bir form) MUTLAKA `fill_form` tool'unu çağır.
                - 'values' nesnesi içerisine forma girilecek alan adlarını ve değerlerini key-value olarak ekle (Örn: {"firstName": "Ahmet", "tcNo": "12345678901", "birthDate": "2000-01-15"} veya {"branch": "Matematik"}).
                - Eğer kullanıcının bulunduğu sayfa veya işlem yapılan form belirli ise 'target' parametresini ayarla (örn: "studentForm", "teacherForm"), aksi halde boş bırakabilirsin.
                - Mevcut formda zaten bulunan değişmemiş bilgileri tekrar göndermene gerek yok, yalnızca yeni ve güncellenmiş bilgileri ilet.
                """
            }
        };

        foreach (var msg in request.Messages)
        {
            if (msg.Role == "assistant" && msg.ToolCalls.Count > 0)
            {
                deepSeekMessages.Add(new
                {
                    role = "assistant",
                    content = msg.Content,
                    tool_calls = msg.ToolCalls.Select(toolCall => new
                    {
                        id = toolCall.Id,
                        type = "function",
                        function = new
                        {
                            name = toolCall.Name,
                            arguments = toolCall.Arguments
                        }
                    }).ToArray()
                });

                continue;
            }

            if (msg.Role == "tool")
            {
                deepSeekMessages.Add(new
                {
                    role = "tool",
                    tool_call_id = msg.ToolCallId,
                    content = msg.Content
                });

                continue;
            }

            deepSeekMessages.Add(new
            {
                role = msg.Role,
                content = msg.Content
            });
        }

        var tools = request.Tools
            .Select(tool => new
            {
                type = "function",
                function = new
                {
                    name = tool.Name,
                    description = tool.Description,
                    parameters = tool.Parameters
                }
            })
            .ToList();

        var requestBody = new
        {
            model,
            messages = deepSeekMessages,
            tools,
            tool_choice = "auto",
            stream = false,
            temperature = 0.7
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "chat/completions");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                apiKey.Trim());

        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        using var response =
            await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

        var json =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"DeepSeek API hatası " +
                $"(HTTP {(int)response.StatusCode}): {json}");
        }

        var deepSeekResponse =
            JsonSerializer.Deserialize<DeepSeekResponse>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (deepSeekResponse is null ||
            deepSeekResponse.Choices.Count == 0)
        {
            throw new InvalidOperationException(
                "DeepSeek geçerli bir response döndürmedi.");
        }

        var responseMessage =
            deepSeekResponse.Choices[0].Message;

        var aiResponse = new AiResponse
        {
            Content = responseMessage.Content
        };

        foreach (var toolCall in responseMessage.ToolCalls)
        {
            aiResponse.ToolCalls.Add(
                new AiToolCall
                {
                    Id = toolCall.Id,
                    Name = toolCall.Function.Name,
                    Arguments = toolCall.Function.Arguments
                });
        }

        Console.WriteLine(
            $"Tool call sayısı: {responseMessage.ToolCalls.Count}");

        foreach (var toolCall in responseMessage.ToolCalls)
        {
            Console.WriteLine($"Tool: {toolCall.Function.Name}");
            Console.WriteLine($"Arguments: {toolCall.Function.Arguments}");
        }

        return aiResponse;
    }
}