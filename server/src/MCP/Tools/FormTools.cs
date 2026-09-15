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
    "Kullanıcının formundaki alanları genel (global) ve dinamik olarak doldurmak veya güncellemek için kullanılır. " +
    "Kullanıcı herhangi bir form bilgisi verdiğinde (öğrenci, öğretmen, ders, iletişim, kayıt vb. hangi form olursa olsun) bu tool MUTLAKA çağrılmalıdır. " +
    "Doldurulacak alanlar 'values' sözlüğünde anahtar-değer (key-value) çiftleri olarak iletilir. " +
    "Örnek: values: {'firstName': 'Ahmet', 'lastName': 'Yılmaz', 'tcNo': '12345678901', 'email': 'ahmet@example.com'}. " +
    "Tarih alanları YYYY-MM-DD formatında iletilmelidir. " +
    "Kullanıcı 'formu doldur', 'forma aktar', 'bilgilerim şunlar' dediğinde bu tool kullanılır.")]
    public static FormPatchResult FillForm(
    [Description("Forma aktarılacak alan ve değer çiftleri sözlüğü.")]
    Dictionary<string, object?> values,
    [Description("Hedef form kimliği (isteğe bağlı, örn: 'studentForm', 'teacherForm' veya sayfa bağlamından otomatik belirlenir).")]
    string? target = null)
    {
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

    [McpServerTool(UseStructuredContent = true)]
    [Description(
    "Kullanıcının kişisel bilgi formundaki alanlarını doldurmak veya güncellemek için kullanılır. " +
    "Kullanıcı bir veya daha fazla form bilgisi verdiğinde bu tool MUTLAKA çağrılmalıdır. " +
    "Kullanıcı daha önce doldurulmuş alanlara ek olarak yeni bir bilgi verdiğinde yalnızca yeni bilgileri göndermek yeterlidir. " +
    "Kullanıcı 'formu doldur', 'forma ekle', 'kaydetme, forma aktar' veya benzer bir ifade kullandığında verilen bilgileri forma aktarmak için bu tool kullanılmalıdır. " +
    "Veritabanına kayıt yapmaz. " +
    "birthDate değeri YYYY-MM-DD formatında gönderilmelidir.")]
    public static FormPatchResult FillStudentForm(
    string? firstName = null,
    string? lastName = null,
    string? tcNo = null,
    string? email = null,
    string? motherName = null,
    string? fatherName = null,
    string? birthDate = null)
    {
        var values = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(firstName)) values["firstName"] = firstName;
        if (!string.IsNullOrWhiteSpace(lastName)) values["lastName"] = lastName;
        if (!string.IsNullOrWhiteSpace(tcNo)) values["tcNo"] = tcNo;
        if (!string.IsNullOrWhiteSpace(email)) values["email"] = email;
        if (!string.IsNullOrWhiteSpace(motherName)) values["motherName"] = motherName;
        if (!string.IsNullOrWhiteSpace(fatherName)) values["fatherName"] = fatherName;
        if (!string.IsNullOrWhiteSpace(birthDate)) values["birthDate"] = birthDate;

        return FillForm(values, "studentForm");
    }

    // [McpServerTool(UseStructuredContent = true)]
    // [Description(
    // "Kullanıcının öğretmen ekleme formundaki alanlarını doldurmak veya güncellemek için kullanılır. " +
    // "Kullanıcı öğretmen bilgisi (ad, soyad, tc, e-posta, branş, anne adı, baba adı, doğum tarihi) verdiğinde veya öğretmen formundayken bu tool MUTLAKA çağrılmalıdır. " +
    // "Veritabanına kayıt yapmaz. " +
    // "birthDate değeri YYYY-MM-DD formatında gönderilmelidir.")]
    // public static FormPatchResult FillTeacherForm(
    // string? firstName = null,
    // string? lastName = null,
    // string? tcNo = null,
    // string? email = null,
    // string? branch = null,
    // string? motherName = null,
    // string? fatherName = null,
    // string? birthDate = null)
    // {
    //     var values = new Dictionary<string, object?>();
    //     if (!string.IsNullOrWhiteSpace(firstName)) values["firstName"] = firstName;
    //     if (!string.IsNullOrWhiteSpace(lastName)) values["lastName"] = lastName;
    //     if (!string.IsNullOrWhiteSpace(tcNo)) values["tcNo"] = tcNo;
    //     if (!string.IsNullOrWhiteSpace(email)) values["email"] = email;
    //     if (!string.IsNullOrWhiteSpace(branch)) values["branch"] = branch;
    //     if (!string.IsNullOrWhiteSpace(motherName)) values["motherName"] = motherName;
    //     if (!string.IsNullOrWhiteSpace(fatherName)) values["fatherName"] = fatherName;
    //     if (!string.IsNullOrWhiteSpace(birthDate)) values["birthDate"] = birthDate;

    //     return FillForm(values, "teacherForm");
    // }


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

    // Normalizes the birthDate to the format "yyyy-MM-dd" if possible. Returns null if the input is null or whitespace.
    private static string? NormalizeBirthDate(string? birthDate) => NormalizeDate(birthDate);
}