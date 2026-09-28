using System.ComponentModel;
using System.Text.Json;
using MCP.Server.Manifest;
using MCP.Server.Models;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Application.MCP.Tools;

[McpServerToolType]
public static class FormTools
{
    [McpServerTool(UseStructuredContent = true)]
    [Description(
    "Kullanıcının formundaki alanları genel (global) ve dinamik olarak doldurmak veya güncellemek için kullanılır. " +
    "Kullanıcı herhangi bir form bilgisi verdiğinde bu tool MUTLAKA çağrılmalıdır. " +
    "Doldurulacak alanlar 'values' sözlüğünde, formun alan adları (name) anahtar olarak iletilir. " +
    "Örnek: values: {'firstName': 'Ahmet', 'lastName': 'Yılmaz', 'tcNo': '12345678901'}. " +
    "Alan adları ekran özetindeki form verisinden veya get_page_schema'dan alınır; uydurulmamalıdır. " +
    "Tarih alanları YYYY-MM-DD formatında iletilmelidir. Veritabanına kayıt yapmaz.")]
    public static FormPatchResult FillForm(
    AppManifestStore manifest,
    [Description("Forma aktarılacak alan ve değer çiftleri sözlüğü.")]
    Dictionary<string, object?> values,
    [Description("Hedef form kimliği (örn: 'studentForm', 'teacherForm'). Boş bırakılırsa kullanıcının bulunduğu sayfanın formu kullanılır.")]
    string? target = null)
    {
        if (!string.IsNullOrWhiteSpace(target))
        {
            ValidateAgainstManifest(manifest, target, values);
        }

        var normalizedData = new Dictionary<string, object?>();

        if (values != null)
        {
            foreach (var (key, value) in values)
            {
                if (value is null)
                    continue;

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
        }

        return new FormPatchResult
        {
            Target = target,
            Data = normalizedData
        };
    }

    // McpException mesajı modele iletilir; model doğru form/alan adlarıyla tekrar deneyebilir.
    private static void ValidateAgainstManifest(
        AppManifestStore manifest,
        string target,
        Dictionary<string, object?>? values)
    {
        var form = manifest.FindForm(target)
            ?? throw new McpException(
                $"Bilinmeyen form: '{target}'. Geçerli formlar: " +
                string.Join(", ", manifest.Manifest.Forms.Select(item => item.Id)));

        var unknownFields = (values?.Keys ?? Enumerable.Empty<string>())
            .Where(key => form.Fields.All(field => field.Name != key))
            .ToList();

        if (unknownFields.Count > 0)
        {
            throw new McpException(
                $"'{form.Id}' formunda olmayan alan(lar): {string.Join(", ", unknownFields)}. " +
                $"Geçerli alanlar: {string.Join(", ", form.Fields.Select(field => field.Name))}");
        }
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
