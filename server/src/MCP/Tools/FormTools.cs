using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace Application.MCP.Tools;

[McpServerToolType]
public static class FormTools
{
    [McpServerTool]
    [Description("Kişisel bilgi formunun alanlarını ve bu alanların ne amaçla kullanıldığını döndürür.")]
    public static string GetFormSchema()
    {
        return """
        {
            "type": "information",
            "form": "student",
            "fields": [
                {
                    "name": "firstName",
                    "label": "Ad",
                    "type": "text"
                },
                {
                    "name": "lastName",
                    "label": "Soyad",
                    "type": "text"
                },
                {
                    "name": "tcNo",
                    "label": "TC Kimlik No",
                    "type": "text"
                },
                {
                    "name": "email",
                    "label": "E-posta",
                    "type": "email"
                },
                {
                    "name": "motherName",
                    "label": "Anne Adı",
                    "type": "text"
                },
                {
                    "name": "fatherName",
                    "label": "Baba Adı",
                    "type": "text"
                },
                {
                    "name": "birthDate",
                    "label": "Doğum Tarihi",
                    "type": "date"
                }
            ]
        }
        """;
    }

    [McpServerTool]
    [Description(
    "Kullanıcının konuşma sırasında açıkça verdiği kişisel bilgileri " +
    "kişisel bilgi formuna aktarmak için kullanılır. " +
    "Veritabanına kayıt yapmaz. " +
    "birthDate değeri her zaman YYYY-MM-DD formatında olmalıdır. " +
    "Örneğin 12.02.2000 değeri 2000-02-12 olarak gönderilmelidir.")]
    public static string FillStudentForm(
    string? firstName = null,
    string? lastName = null,
    string? tcNo = null,
    string? email = null,
    string? motherName = null,
    string? fatherName = null,
    string? birthDate = null)
    {
        var data = new
        {
            firstName,
            lastName,
            tcNo,
            email,
            motherName,
            fatherName,
            birthDate = NormalizeBirthDate(birthDate)
        };

        var result = new
        {
            type = "form_patch",
            target = "studentForm",
            data
        };

        return JsonSerializer.Serialize(result);
    }


    [McpServerTool]
    [Description("Kisisel bilgi formunodaki dolu veya bos oldugunu gosterir." + "Form degerlerini degistirmez veya veritabanina kaydetmez.")]
    public static string GetFormStatus(
        string? firstName = null,
        string? lastName = null,
        string? tcNo = null,
        string? email = null,
        string? motherName = null,
        string? fatherName = null,
        string? birthDate = null)
    {
        var status = new
        {
            firstName = !string.IsNullOrWhiteSpace(firstName),
            lastName = !string.IsNullOrWhiteSpace(lastName),
            tcNo = !string.IsNullOrWhiteSpace(tcNo),
            email = !string.IsNullOrWhiteSpace(email),
            motherName = !string.IsNullOrWhiteSpace(motherName),
            fatherName = !string.IsNullOrWhiteSpace(fatherName),
            birthDate = !string.IsNullOrWhiteSpace(birthDate)
        };

        return JsonSerializer.Serialize(status);
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