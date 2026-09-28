using DisplayAttribute = System.ComponentModel.DataAnnotations.DisplayAttribute;
using System.Reflection;
using System.Text.Json;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.Forms;

/// <summary>
/// [Display(Name, Description)] varsa Swagger şemasındaki property'ye title/description yazar.
/// Asistan, kullanıcının bulunmadığı bir sayfanın alanlarını bu etiketlerle anlatır.
/// Attribute yoksa bir şey yapmaz; etiket alan adına düşer.
/// </summary>
public sealed class DisplaySchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties is null || schema.Properties.Count == 0)
        {
            return;
        }

        foreach (var property in context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetCustomAttribute<DisplayAttribute>() is not { } display ||
                !schema.Properties.TryGetValue(JsonNamingPolicy.CamelCase.ConvertName(property.Name), out var propertySchema) ||
                propertySchema is not OpenApiSchema concrete)
            {
                continue;
            }

            concrete.Title ??= display.GetName();
            concrete.Description ??= display.GetDescription();
        }
    }
}
