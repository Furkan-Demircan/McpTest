using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using Application.AI;
using Application.AI.Contracts;
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

    private static string FormatScreen(ScreenSnapshot? screen)
    {
        if (screen is null || screen.Elements.Count == 0)
        {
            return "- (ekran özeti yok)";
        }

        return string.Join('\n', screen.Elements.Select(element =>
        {
            var states = new List<string>();
            if (element.Required == true) states.Add("zorunlu");
            if (element.Filled is not null) states.Add(element.Filled.Value ? "dolu" : "boş");
            if (element.Disabled == true) states.Add("pasif");

            var state = states.Count > 0 ? $" — {string.Join(", ", states)}" : string.Empty;
            return $"- {element.Id} [{element.Kind}] \"{element.Label}\"{state}";
        }));
    }

    public async Task<AiResponse> ChatAsync(
        AiRequest request,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["DeepSeek:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = _configuration["DEEPSEEK_API_KEY"];
        }

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

                Uygulamayı nereden bilirsin (bilgi kaynakları):
                - Ekran özeti (bağlam mesajında): kullanıcının ŞU AN gördüğü elemanlar,
                  kimlikleri ve dolu/boş durumları. Bulunduğu sayfa için tek doğru kaynak budur.
                - `list_app_pages` / `get_page_schema`: sayfalar ve başka bir sayfadaki alanlar,
                  zorunluluklar, validasyon kuralları. Sayfa listesini ezbere bilmezsin; gerekirse sor.
                - `search_app_knowledge`: süreç bilgisi (adım sırası, iş kuralları, bilinen kısıtlar).

                Önemli sınırlar:
                - Veritabanına doğrudan erişemezsin.
                - Kullanıcı ekleyemezsin.
                - Kullanıcı silemezsin.
                - Kullanıcı bilgilerini güncelleyemezsin.
                - CRUD işlemlerini kendin gerçekleştiremezsin.
                - Gerçekleştirmediğin bir işlemi gerçekleştirmiş gibi söyleme.
                - Bilmediğin bilgileri uydurma.

                Boş string değerleri doldurulmamış kabul et.

                Kullanıcıyı yönlendirme (destek asistanı davranışı):
                - Kullanıcı bir işlemi nasıl yapacağını sorarsa, bilmediğini/bulamadığını söylerse
                  veya bir hata aldığını anlatırsa ÖNCE `search_app_knowledge` tool'unu çağır.
                - Cevabını yalnızca dönen rehbere, ekran özetine ve sayfa şemasına dayandır.
                  Bunlarda olmayan adım, alan veya özellik uydurma; bulunamazsa açıkça söyle.
                - Alan listesi veya kurallar gerekiyorsa: kullanıcı o sayfadaysa ekran özetini,
                  değilse `get_page_schema` sonucunu kullan. Ekrandaki etiketlerle konuş, alan
                  adlarını (firstName vb.) ve kimlikleri kullanıcıya gösterme.
                - Adımları kısa, numaralı ve sade bir dille anlat; sistemi ilk kez kullanan
                  bir personele anlatır gibi, teknik terim kullanmadan.
                - Kullanıcı işlemi şimdi yapmak istiyorsa (örn. "yeni öğrenci eklemem lazım,
                  nasıl yapacağımı bilmiyorum"): gerekiyorsa rehberdeki sayfa kimliğiyle
                  `navigate_to_page` çağır, ardından `highlight_element` ile başlaması gereken
                  ilk alanı işaretle. Zaten o sayfadaysa sadece işaretle.
                - highlight_element kimliğini uydurma: bulunduğu sayfa için ekran özetinden,
                  navigasyondan sonraki sayfa için rehberdeki [id: ...] veya get_page_schema'dan al.
                - Tool çağrılarını gereksiz yere tek tek yapma; birbirine bağlı olmayanları
                  (örn. navigate_to_page + highlight_element) aynı turda birlikte çağır.
                - Anlatımın sonunda bilgileri sana yazarak veya sesle söyleyerek formu
                  doldurtabileceğini, ama kaydet butonuna kendisinin basması gerektiğini hatırlat.
                - Kullanıcı sadece bilgi istiyorsa (işlem yapmak istediği belli değilse)
                  sayfaya kendin götürme; anlat ve götürmeyi teklif et.

                Form doldurma / güncelleme kuralları:
                - Kullanıcı herhangi bir form bilgisi verdiğinde MUTLAKA `fill_form` tool'unu çağır.
                - 'values' anahtarları formun alan adlarıdır: aktif form için bağlamdaki form verisinin
                  anahtarları, başka bir form için get_page_schema'daki field.name değerleri.
                - 'target' boşsa kullanıcının bulunduğu sayfanın formu kullanılır. Bilgi başka bir
                  forma aitse (örn. ana sayfadayken öğretmen bilgisi verildi) önce o sayfaya
                  `navigate_to_page` ile git ve 'target' olarak o sayfanın formId'sini ver.
                - Formsuz bir sayfadaysan ve hangi forma ait olduğu belli değilse kullanıcıya sor.
                - Mevcut formda zaten bulunan değişmemiş bilgileri tekrar göndermene gerek yok, yalnızca yeni ve güncellenmiş bilgileri ilet.

                Tool hataları:
                - Bir tool {"error": ...} döndürürse hatayı oku, mümkünse düzeltilmiş argümanlarla tekrar dene; değilse kullanıcıya açıkça bildir.
                """
            },
            new
            {
                role = "system",
                content = $"""
                Mevcut bağlam (her istekte istemciden gelir):
                - Kullanıcının bulunduğu sayfa: {request.CurrentPage ?? "bilinmiyor"}
                - Sayfanın başlığı: {request.Screen?.Heading ?? "bilinmiyor"}
                - Aktif form: {request.ActiveFormId ?? "yok (bu sayfada form yok)"}
                - Aktif formun güncel verisi (boş string = doldurulmamış):
                {currentFormData}

                Ekran özeti — kullanıcının şu an gördüğü etkileşimli elemanlar
                (format: kimlik [tür] "etiket" — durum):
                {FormatScreen(request.Screen)}
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