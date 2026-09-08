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

    public async Task<AiChatResponse> ChatAsync(
    List<ChatMessage> messages,
    CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["DeepSeek:ApiKey"]
            ?? _configuration["DEEPSEEK_API_KEY"];

        if (string.IsNullOrWhiteSpace(apiKey) ||
            apiKey == "your_deepseek_api_key_here")
        {
            throw new InvalidOperationException(
                "DeepSeek API anahtarı yapılandırılmamış. " +
                "Lütfen kök dizindeki .env dosyasında " +
                "'DEEPSEEK_API_KEY' alanına gerçek API anahtarınızı giriniz.");
        }

        var model = _configuration["DeepSeek:Model"]
            ?? "deepseek-chat";

        var deepSeekMessages = new List<object>
    {
        new
        {
            role = "system",
            content = """
                Sen, kullanıcı yönetim ve kişisel bilgi formu sisteminde çalışan bir yapay zeka asistanısın.

                Görevin:
                - Kullanıcıyla Türkçe ve kısa şekilde iletişim kurmak.
                - Kullanıcının taleplerini anlamak ve uygun şekilde yönlendirmek.
                - Öğrenci/kullanıcı bilgi formunun doldurulmasına yardımcı olmak.
                - Kullanıcının verdiği bilgilerden form alanlarını anlamak.
                - Eksik bilgiler varsa bunları kullanıcıdan istemek.
                - Kullanıcının vermediği bilgileri tahmin etmemek veya uydurmamak.
                - Kullanıcıya formdaki gerekli alanlar hakkında bilgi vermek.

                Form alanları:
                - Ad
                - Soyad
                - 11 haneli TC Kimlik Numarası
                - E-posta
                - Anne Adı
                - Baba Adı
                - Doğum Tarihi

                Sınırların:
                - Veritabanına doğrudan erişemezsin.
                - Kullanıcı kaydedemezsin.
                - Kullanıcı silemezsin.
                - Kullanıcı bilgilerini güncelleyemezsin.
                - Kullanıcı adına CRUD işlemi gerçekleştiremezsin.
                - Gerçekleştirmediğin bir işlemi gerçekleştirmiş gibi söyleyemezsin.
                - Form verilerinin nihai doğrulamasını sen yapmazsın; backend doğrulaması esas alınır.

                Yanıt kuralları:
                - Türkçe yanıt ver.
                - Kısa, net ve doğal konuş.
                - Gereksiz teknik detay verme.
                - Kullanıcı bir işlem yapmak istediğinde, işlemi kendin gerçekleştirmek yerine gerekli bilgileri ve sonraki adımı belirt.
                Yanıt formatı:

                Yanıtını her zaman JSON formatında üret.

                JSON şu alanları içermelidir:

                {
                "message": "Kullanıcıya gösterilecek Türkçe mesaj",
                "formPatch": {
                    "firstName": null,
                    "lastName": null,
                    "tcNo": null,
                    "email": null,
                    "motherName": null,
                    "fatherName": null,
                    "birthDate": null
                },
                "missingFields": []
                }

                JSON kuralları:
                - Sadece geçerli JSON döndür.
                - JSON dışında açıklama, markdown veya metin döndürme.
                - message kullanıcıya gösterilecek doğal Türkçe mesajdır.
                - formPatch sadece kullanıcının açıkça verdiği bilgileri içermelidir.
                - Kullanıcının vermediği bilgileri tahmin etme veya uydurma.
                - Kullanıcı bir bilgi verdiğinde ilgili form alanını formPatch içerisinde doldur.
                - missingFields henüz verilmemiş alanları içermelidir.
                - Alan isimlerini değiştirme.
                
                """
        }
    };

        foreach (var message in messages)
        {
            deepSeekMessages.Add(new
            {
                role = message.Role,
                content = message.Content
            });
        }

        var requestBody = new
        {
            model = model,
            messages = deepSeekMessages,
            response_format = new
            {
                type = "json_object"
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

        var json =
            await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"DeepSeek API hatası (HTTP {(int)response.StatusCode}): {json}");
        }

        using var document = JsonDocument.Parse(json);

        if (document.RootElement.TryGetProperty("choices", out var choices) &&
            choices.GetArrayLength() > 0 &&
            choices[0].TryGetProperty("message", out var messageElement) &&
            messageElement.TryGetProperty("content", out var contentElement))
        {
            var content = contentElement.GetString();

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException(
                    "DeepSeek boş bir yanıt döndürdü.");
            }

            var aiResponse =
                JsonSerializer.Deserialize<AiChatResponse>(
                    content,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (aiResponse is null)
            {
                throw new InvalidOperationException(
                    "DeepSeek yanıtı beklenen JSON formatında değil.");
            }

            return aiResponse;
        }

        throw new InvalidOperationException(
            "DeepSeek modelinden geçerli bir yanıt alınamadı.");
    }
}