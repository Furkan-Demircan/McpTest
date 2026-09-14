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
    "Kullanıcının kişisel bilgi formundaki alanlarını doldurmak veya güncellemek için kullanılır. " +
    "Kullanıcı bir veya daha fazla form bilgisi verdiğinde bu tool MUTLAKA çağrılmalıdır. " +
    "Kullanıcı daha önce doldurulmuş alanlara ek olarak yeni bir bilgi verdiğinde yalnızca yeni bilgileri göndermek yeterlidir. " +
    "Kullanıcı 'formu doldur', 'forma ekle', 'kaydetme, forma aktar' veya benzer bir ifade kullandığında verilen bilgileri forma aktarmak için bu tool kullanılmalıdır. " +
    "Veritabanına kayıt yapmaz. " +
    "birthDate değeri YYYY-MM-DD formatında gönderilmelidir. " +
    "Örneğin 11.02.2013 değeri 2013-02-11 olarak gönderilmelidir.")]
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
            Target = "studentForm",
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
    "Kullanıcının öğretmen ekleme formundaki alanlarını doldurmak veya güncellemek için kullanılır. " +
    "Kullanıcı öğretmen bilgisi (ad, soyad, tc, e-posta, branş, anne adı, baba adı, doğum tarihi) verdiğinde veya öğretmen formundayken bu tool MUTLAKA çağrılmalıdır. " +
    "Veritabanına kayıt yapmaz. " +
    "birthDate değeri YYYY-MM-DD formatında gönderilmelidir.")]
    public static FormPatchResult FillTeacherForm(
    string? firstName = null,
    string? lastName = null,
    string? tcNo = null,
    string? email = null,
    string? branch = null,
    string? motherName = null,
    string? fatherName = null,
    string? birthDate = null)
    {
        return new FormPatchResult
        {
            Target = "teacherForm",
            Data = new FormPatchData
            {
                FirstName = firstName,
                LastName = lastName,
                TcNo = tcNo,
                Email = email,
                Branch = branch,
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