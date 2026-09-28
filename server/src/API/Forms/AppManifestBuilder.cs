using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Forms;
using MCP.Server.Manifest;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace API.Forms;

/// <summary>
/// Uygulama manifest'ini koddan üretir:
/// - Formlar: [AppForm] işaretli action'lar (opt-in). Alanlar action'ın [FromBody] DTO'sunun
///   DataAnnotations'ından ([Display], [Required], [StringLength], [RegularExpression],
///   [EmailAddress], [Suggestions]), submit adresi ApiExplorer'daki route'tan gelir.
/// - Özel sayfalar: AppPageRegistry. Ana sayfa menüsü NavLabel'ı olan sayfalardan oluşur.
/// Tutarsızlıklar (tekrarlanan kimlik/path, DTO'suz form) Issues olarak döner.
/// </summary>
public sealed class AppManifestBuilder(
    IApiDescriptionGroupCollectionProvider apiExplorer,
    AppPageRegistry registry)
{
    public (AppManifest Manifest, IReadOnlyList<string> Issues) Build()
    {
        var issues = new List<string>();
        var forms = new List<FormDefinition>();
        var formPages = new List<PageDefinition>();

        foreach (var description in apiExplorer.ApiDescriptionGroups.Items.SelectMany(group => group.Items))
        {
            if (description.ActionDescriptor is not ControllerActionDescriptor action ||
                action.MethodInfo.GetCustomAttribute<AppFormAttribute>() is not { } appForm)
            {
                continue;
            }

            var body = description.ParameterDescriptions
                .FirstOrDefault(parameter => parameter.Source == BindingSource.Body);

            if (body is null)
            {
                issues.Add($"[AppForm(\"{appForm.Id}\")] {action.DisplayName}: [FromBody] DTO parametresi yok, form üretilmedi.");
                continue;
            }

            var pageId = appForm.PageId ?? appForm.Id;
            var submitId = $"{appForm.Id}-submit";
            var resetId = $"{appForm.Id}-reset";

            forms.Add(new FormDefinition
            {
                Id = appForm.Id,
                Title = appForm.Title,
                PageId = pageId,
                Module = appForm.Module,
                Submit = new FormSubmit
                {
                    Method = description.HttpMethod ?? "POST",
                    Url = "/" + description.RelativePath
                },
                SubmitElementId = submitId,
                ResetElementId = resetId,
                Fields = BuildFields(appForm.Id, body.Type)
            });

            formPages.Add(new PageDefinition
            {
                Id = pageId,
                Path = appForm.Path,
                Aliases = [.. appForm.Aliases],
                Title = appForm.Title,
                Description = appForm.Description ?? string.Empty,
                Module = appForm.Module,
                NavLabel = appForm.NavLabel,
                FormId = appForm.Id,
                Elements =
                [
                    new PageElement { Id = submitId, Label = appForm.SubmitLabel, Kind = "button" },
                    new PageElement { Id = resetId, Label = "Temizle", Kind = "button" }
                ]
            });
        }

        var home = registry.Pages.FirstOrDefault(page => page.Path == "/");
        var otherPages = registry.Pages.Where(page => page != home).ToList();
        var pages = new List<PageDefinition>();

        if (home is not null)
        {
            // Menü: önce formlar, sonra diğer sayfalar
            home.Elements =
            [
                .. home.Elements.Where(element => element.TargetPageId is null),
                .. formPages.Concat(otherPages)
                    .Where(page => page.NavLabel is not null)
                    .Select(page => new PageElement
                    {
                        Id = $"nav-{page.Id}",
                        Label = page.NavLabel!,
                        Kind = "link",
                        TargetPageId = page.Id
                    })
            ];
            pages.Add(home);
        }

        pages.AddRange(formPages);
        pages.AddRange(otherPages);

        var manifest = new AppManifest { Pages = pages, Forms = forms };
        issues.AddRange(Validate(manifest));

        return (manifest, issues);
    }

    private static List<FieldDefinition> BuildFields(string formId, Type dtoType) =>
        dtoType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanWrite && property.GetCustomAttribute<JsonIgnoreAttribute>() is null)
            .Select((property, index) => (Property: property, Display: property.GetCustomAttribute<DisplayAttribute>(), Index: index))
            // Kalıtımda türetilmiş sınıfın alanları önce gelir; sırayı Display.Order belirler.
            .OrderBy(item => item.Display?.GetOrder() ?? int.MaxValue)
            .ThenBy(item => item.Index)
            .Select(item => BuildField(formId, item.Property, item.Display))
            .ToList();

    private static FieldDefinition BuildField(string formId, PropertyInfo property, DisplayAttribute? display)
    {
        var name = JsonNamingPolicy.CamelCase.ConvertName(property.Name);
        var label = display?.GetName() ?? property.Name;
        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        var isEmail = property.GetCustomAttribute<EmailAddressAttribute>() is not null;
        var required = property.GetCustomAttribute<RequiredAttribute>();
        var rules = new List<FieldRule>();

        if (property.GetCustomAttribute<StringLengthAttribute>() is { } stringLength)
        {
            if (stringLength.MinimumLength > 0)
            {
                rules.Add(new FieldRule { Kind = "minLength", Value = stringLength.MinimumLength, Message = stringLength.FormatErrorMessage(label) });
            }

            rules.Add(new FieldRule { Kind = "maxLength", Value = stringLength.MaximumLength, Message = stringLength.FormatErrorMessage(label) });
        }

        if (property.GetCustomAttribute<RegularExpressionAttribute>() is { } regex)
        {
            rules.Add(new FieldRule { Kind = "pattern", Pattern = regex.Pattern, Message = regex.FormatErrorMessage(label) });
        }

        if (property.GetCustomAttribute<EmailAddressAttribute>() is { } email)
        {
            rules.Add(new FieldRule { Kind = "email", Message = email.FormatErrorMessage(label) });
        }

        return new FieldDefinition
        {
            Name = name,
            Label = label,
            Type = type == typeof(DateOnly) || type == typeof(DateTime) || type == typeof(DateTimeOffset)
                ? "date"
                : isEmail ? "email" : "text",
            ElementId = $"{formId}-{name}",
            Required = required is not null,
            RequiredMessage = required?.FormatErrorMessage(label),
            Rules = rules.Count > 0 ? rules : null,
            Options = property.GetCustomAttribute<SuggestionsAttribute>()?.Values.ToList(),
            Hint = display?.GetDescription()
        };
    }

    private static IEnumerable<string> Validate(AppManifest manifest)
    {
        static IEnumerable<string> Duplicates(IEnumerable<string> values) =>
            values.GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);

        foreach (var id in Duplicates(manifest.Forms.Select(form => form.Id)))
            yield return $"Tekrarlanan form kimliği: {id}";

        foreach (var id in Duplicates(manifest.Pages.Select(page => page.Id)))
            yield return $"Tekrarlanan sayfa kimliği: {id}";

        foreach (var path in Duplicates(manifest.Pages.SelectMany(page => page.Aliases.Prepend(page.Path))))
            yield return $"Tekrarlanan path/alias: {path}";

        var elementIds = manifest.Pages.SelectMany(page => page.Elements.Select(element => element.Id))
            .Concat(manifest.Forms.SelectMany(form => form.Fields.Select(field => field.ElementId)));

        foreach (var id in Duplicates(elementIds))
            yield return $"Tekrarlanan eleman kimliği: {id}";
    }
}
