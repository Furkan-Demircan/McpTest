using System.Text.Json;
using MCP.Server.Catalog;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;

namespace API.Catalog;

/// <summary>
/// Bir endpoint'in request body şemasını uygulamanın kendi Swagger dokümanından okur ve
/// asistanın anlayacağı alan listesine çevirir. Backend'de ek işaretleme gerekmez:
/// Swashbuckle DataAnnotations'ı zaten şemaya yazar (required, maxLength, pattern, format).
/// Swagger yalnızca sözleşme kaynağıdır: alan adı, tip, zorunluluk ve kurallar okunur.
/// Etiket, ipucu veya kullanım bilgisi buradan alınmaz; o Application Knowledge katmanındadır.
/// Doküman JSON'a serileştirilip okunur; Microsoft.OpenApi nesne modelinin sürüm
/// farklarından etkilenmemek için.
/// </summary>
public sealed class SwaggerFormSchemaProvider(ISwaggerProvider swaggerProvider)
{
    private const string DocumentName = "v1";

    private readonly Lazy<JsonElement> _document = new(() => Load(swaggerProvider));

    private static JsonElement Load(ISwaggerProvider swaggerProvider)
    {
        var document = swaggerProvider.GetSwagger(DocumentName);
        using var writer = new StringWriter();
        document.SerializeAsV3(new OpenApiJsonWriter(writer));
        return JsonDocument.Parse(writer.ToString()).RootElement.Clone();
    }

    /// <summary>"POST /api/Users" gibi bir endpoint'in body alanları; bulunamazsa null.</summary>
    public List<FieldDefinition>? GetRequestFields(string endpoint)
    {
        var parts = endpoint.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !_document.Value.TryGetProperty("paths", out var paths))
        {
            return null;
        }

        var (method, url) = (parts[0].ToLowerInvariant(), parts[1]);

        var path = paths.EnumerateObject()
            .FirstOrDefault(item => item.Name.Equals(url, StringComparison.OrdinalIgnoreCase));

        if (path.Value.ValueKind != JsonValueKind.Object ||
            !path.Value.TryGetProperty(method, out var operation) ||
            !TryGetPath(operation, out var schema, "requestBody", "content", "application/json", "schema"))
        {
            return null;
        }

        var (properties, required) = CollectProperties(Resolve(schema));

        return properties
            .Select(property => ToField(property.Name, Resolve(property.Value), required.Contains(property.Name)))
            .ToList();
    }

    private static FieldDefinition ToField(string name, JsonElement schema, bool required)
    {
        var format = GetString(schema, "format");
        var rules = new List<FieldRule>();

        if (GetInt(schema, "minLength") is > 0 and var minLength)
            rules.Add(new FieldRule { Kind = "minLength", Value = minLength });

        if (GetInt(schema, "maxLength") is { } maxLength)
            rules.Add(new FieldRule { Kind = "maxLength", Value = maxLength });

        if (GetString(schema, "pattern") is { } pattern)
            rules.Add(new FieldRule { Kind = "pattern", Pattern = pattern });

        if (format == "email")
            rules.Add(new FieldRule { Kind = "email" });

        if (GetInt(schema, "minimum") is { } minimum)
            rules.Add(new FieldRule { Kind = "minimum", Value = minimum });

        if (GetInt(schema, "maximum") is { } maximum)
            rules.Add(new FieldRule { Kind = "maximum", Value = maximum });

        return new FieldDefinition
        {
            Name = name,
            Type = (GetString(schema, "type"), format) switch
            {
                (_, "email") => "email",
                (_, "date" or "date-time") => "date",
                ("integer" or "number", _) => "number",
                _ => "text"
            },
            ElementId = name,
            Required = required,
            Rules = rules.Count > 0 ? rules : null
        };
    }

    // allOf (kalıtım) varsa parçaların alanlarını birleştirir
    private (List<JsonProperty> Properties, HashSet<string> Required) CollectProperties(JsonElement schema)
    {
        var properties = new List<JsonProperty>();
        var required = new HashSet<string>(StringComparer.Ordinal);

        if (schema.TryGetProperty("allOf", out var allOf))
        {
            foreach (var part in allOf.EnumerateArray())
            {
                var (partProperties, partRequired) = CollectProperties(Resolve(part));
                properties.AddRange(partProperties);
                required.UnionWith(partRequired);
            }
        }

        if (schema.TryGetProperty("properties", out var own))
            properties.AddRange(own.EnumerateObject());

        if (schema.TryGetProperty("required", out var ownRequired))
            required.UnionWith(ownRequired.EnumerateArray().Select(item => item.GetString()!));

        return (properties, required);
    }

    // "#/components/schemas/CreateUserDto" referanslarını çözer
    private JsonElement Resolve(JsonElement schema)
    {
        if (!schema.TryGetProperty("$ref", out var reference))
        {
            return schema;
        }

        var segments = reference.GetString()!.TrimStart('#', '/').Split('/');
        return TryGetPath(_document.Value, out var resolved, segments) ? Resolve(resolved) : schema;
    }

    private static bool TryGetPath(JsonElement element, out JsonElement result, params string[] segments)
    {
        result = element;
        foreach (var segment in segments)
        {
            if (result.ValueKind != JsonValueKind.Object || !result.TryGetProperty(segment, out result))
            {
                return false;
            }
        }

        return true;
    }

    private static string? GetString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static int? GetInt(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number &&
        value.TryGetInt32(out var number)
            ? number
            : null;
}
