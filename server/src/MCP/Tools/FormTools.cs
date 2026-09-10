using System.ComponentModel;
using System.Text.Json;
using MCP.Server.Models;
using ModelContextProtocol.Server;

namespace Application.MCP.Tools;

[McpServerToolType]
public static class FormTools
{
    [McpServerTool(UseStructuredContent = true)]
    [Description("Kişisel bilgi formunun alanlarını ve bu alanların ne amaçla kullanıldığını döndürür.")]
    public static FormSchemaResult GetFormSchema()
    {
        return new FormSchemaResult
        {
            Fields =
            [
                new FormField
            {
                Name = "firstName",
                Label = "Ad",
                Type = "text"
            },
            new FormField
            {
                Name = "lastName",
                Label = "Soyad",
                Type = "text"
            },
            new FormField
            {
                Name = "tcNo",
                Label = "TC Kimlik No",
                Type = "text"
            },
            new FormField
            {
                Name = "email",
                Label = "E-posta",
                Type = "email"
            },
            new FormField
            {
                Name = "motherName",
                Label = "Anne Adı",
                Type = "text"
            },
            new FormField
            {
                Name = "fatherName",
                Label = "Baba Adı",
                Type = "text"
            },
            new FormField
            {
                Name = "birthDate",
                Label = "Doğum Tarihi",
                Type = "date"
            }
            ]
        };
    }

    [McpServerTool(UseStructuredContent = true)]
    [Description(
    "Kullanıcının konuşma sırasında açıkça verdiği kişisel bilgileri " +
    "kişisel bilgi formuna aktarmak için kullanılır. " +
    "Veritabanına kayıt yapmaz. " +
    "birthDate değeri her zaman YYYY-MM-DD formatında olmalıdır. " +
    "Örneğin 12.02.2000 değeri 2000-02-12 olarak gönderilmelidir.")]
    public static FormPatchResult FillStudentForm(
    string? firstName = null,
    string? lastName = null,
    string? tcNo = null,
    string? email = null,
    string? motherName = null,
    string? fatherName = null,
    string? birthDate = null)
    {
        return new FormPatchResult
        {
            Data = new FormPatchData
            {
                FirstName = firstName,
                LastName = lastName,
                TcNo = tcNo,
                Email = email,
                MotherName = motherName,
                FatherName = fatherName,
                BirthDate = NormalizeBirthDate(birthDate)
            }
        };
    }


    [McpServerTool(UseStructuredContent = true)]
    [Description(
    "Kişisel bilgi formundaki alanların dolu veya boş olduğunu gösterir. " +
    "Form değerlerini değiştirmez veya veritabanına kaydetmez.")]
    public static FormStatusResult GetFormStatus(
    string? firstName = null,
    string? lastName = null,
    string? tcNo = null,
    string? email = null,
    string? motherName = null,
    string? fatherName = null,
    string? birthDate = null)
    {
        return new FormStatusResult
        {
            FirstName = !string.IsNullOrWhiteSpace(firstName),
            LastName = !string.IsNullOrWhiteSpace(lastName),
            TcNo = !string.IsNullOrWhiteSpace(tcNo),
            Email = !string.IsNullOrWhiteSpace(email),
            MotherName = !string.IsNullOrWhiteSpace(motherName),
            FatherName = !string.IsNullOrWhiteSpace(fatherName),
            BirthDate = !string.IsNullOrWhiteSpace(birthDate)
        };
    }


    // Normalizes the birthDate to the format "yyyy-MM-dd" if possible. Returns null if the input is null or whitespace.
    private static string? NormalizeBirthDate(string? birthDate)
    {
        if (string.IsNullOrWhiteSpace(birthDate))
            return null;

        var formats = new[]
        {
        "yyyy-MM-dd",
        "dd.MM.yyyy",
        "dd/MM/yyyy",
        "dd-MM-yyyy"
    };

        if (DateTime.TryParseExact(
            birthDate,
            formats,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out var date))
        {
            return date.ToString("yyyy-MM-dd");
        }

        return birthDate;
    }
}