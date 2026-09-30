using System.ComponentModel;
using System.Text.Json;
using MCP.Server.Catalog;
using MCP.Server.Models;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Application.MCP.Tools;

[McpServerToolType]
public static class FormTools
{
    // Asistan uygulamanın state'ine değil, kullanıcının gördüğü alanlara yazar (DOM).
    // Hangi alanların yazılabileceğini sunucu ekran özetine / hedef sayfanın şemasına göre doğrular.
    [McpServerTool(UseStructuredContent = true)]
    [Description(
    "Kullanıcının ekranındaki alanlara değer yazar: form alanları, arama kutusu, filtre vb. " +
    "Kullanıcı bir forma girilecek bilgi verdiğinde veya bir alana yazılması gereken bir şey istediğinde kullanılır. " +
    "'values' anahtarları ekran özetindeki alan kimlikleridir (field; yoksa name/id). Başka bir sayfaya " +
    "navigate_to_page ile gidildiyse anahtarlar o sayfanın get_page_schema alan adlarıdır (name). Uydurulmamalıdır. " +
    "Örnek: values: {'firstName': 'Ahmet', 'tcNo': '12345678901'}. " +
    "Tarihler YYYY-MM-DD formatında iletilir. Bir alanı temizlemek için değer olarak boş string (\"\") ver. " +
    "Veritabanına kayıt yapmaz; kaydetmek kullanıcıya aittir.")]
    public static FillFieldsResult FillFields(
    AppCatalogStore catalog,
    [Description("Alan kimliği ve yazılacak değer çiftleri sözlüğü.")]
    Dictionary<string, object?> values)
    {
        if (values is null || values.Count == 0)
        {
            throw new McpException("values boş olamaz.");
        }

        var normalizedData = new Dictionary<string, object?>();

        foreach (var (key, value) in values)
        {
            // null ve "" alanı temizle demektir
            if (value is null)
            {
                normalizedData[key] = string.Empty;
                continue;
            }

            // Tarih kontrolü ve normalizasyonu
            if (value is string strVal &&
                (key.Contains("date", StringComparison.OrdinalIgnoreCase) ||
                key.Contains("tarih", StringComparison.OrdinalIgnoreCase) ||
                strVal.Contains('.') || strVal.Contains('/')))
            {
                var normalizedDate = NormalizeDate(strVal);
                normalizedData[key] = normalizedDate ?? strVal;
            }
            else if (value is JsonElement jsonElem)
            {
                normalizedData[key] = jsonElem.ValueKind switch
                {
                    JsonValueKind.String => (key.Contains("date", StringComparison.OrdinalIgnoreCase) ||
                                            key.Contains("tarih", StringComparison.OrdinalIgnoreCase))
                        ? NormalizeDate(jsonElem.GetString()) ?? jsonElem.GetString()
                        : jsonElem.GetString(),
                    JsonValueKind.Number => jsonElem.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => jsonElem.ToString()
                };
            }
            else
            {
                normalizedData[key] = value;
            }
        }

        return new FillFieldsResult
        {
            Data = new FillFieldsData
            {
                Values = normalizedData,
                FieldPages = normalizedData.Keys.ToDictionary(
                    key => key,
                    key => catalog.FindElements(key).Select(element => element.PageId).Distinct().ToList())
            }
        };
    }

    public static string? NormalizeDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr))
            return null;

        var formats = new[]
        {
            "yyyy-MM-dd",
            "dd.MM.yyyy",
            "dd/MM/yyyy",
            "dd-MM-yyyy"
        };

        if (DateTime.TryParseExact(
            dateStr,
            formats,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out var parsedDate))
        {
            return parsedDate.ToString("yyyy-MM-dd");
        }

        return dateStr;
    }
}
