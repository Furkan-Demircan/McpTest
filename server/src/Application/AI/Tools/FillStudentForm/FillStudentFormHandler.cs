using Application.AI.Form;

namespace Application.AI.Tools;

public class FillStudentFormHandler
{
    private readonly StudentFormValidator _validator;

    public FillStudentFormHandler(
        StudentFormValidator validator)
    {
        _validator = validator;
    }

    public AiChatResponse Handle(
        FillStudentFormArguments arguments,
        Dictionary<string, string?> currentFormData)
    {
        var formPatch = new Dictionary<string, string?>();

        if (!string.IsNullOrWhiteSpace(arguments.FirstName))
            formPatch["firstName"] = arguments.FirstName;

        if (!string.IsNullOrWhiteSpace(arguments.LastName))
            formPatch["lastName"] = arguments.LastName;

        if (!string.IsNullOrWhiteSpace(arguments.TcNo))
            formPatch["tcNo"] = arguments.TcNo;

        if (!string.IsNullOrWhiteSpace(arguments.Email))
            formPatch["email"] = arguments.Email;

        if (!string.IsNullOrWhiteSpace(arguments.MotherName))
            formPatch["motherName"] = arguments.MotherName;

        if (!string.IsNullOrWhiteSpace(arguments.FatherName))
            formPatch["fatherName"] = arguments.FatherName;

        if (!string.IsNullOrWhiteSpace(arguments.BirthDate))
            formPatch["birthDate"] = arguments.BirthDate;

        var mergedFormData =
            new Dictionary<string, string?>(
                currentFormData);

        if (formPatch.TryGetValue(
                "firstName",
                out var firstName))
        {
            mergedFormData["ad"] = firstName;
        }

        if (formPatch.TryGetValue(
                "lastName",
                out var lastName))
        {
            mergedFormData["soyad"] = lastName;
        }

        if (formPatch.TryGetValue(
                "tcNo",
                out var tcNo))
        {
            mergedFormData["tcNo"] = tcNo;
        }

        if (formPatch.TryGetValue(
                "email",
                out var email))
        {
            mergedFormData["email"] = email;
        }

        if (formPatch.TryGetValue(
                "motherName",
                out var motherName))
        {
            mergedFormData["anneAdi"] = motherName;
        }

        if (formPatch.TryGetValue(
                "fatherName",
                out var fatherName))
        {
            mergedFormData["babaAdi"] = fatherName;
        }

        if (formPatch.TryGetValue(
                "birthDate",
                out var birthDate))
        {
            mergedFormData["dogumTarihi"] = birthDate;
        }

        var missingFields =
            _validator.GetMissingFields(
                mergedFormData);

        var message =
            CreateMessage(
                formPatch,
                missingFields);

        return new AiChatResponse
        {
            Message = message,
            FormPatch = formPatch,
            MissingFields = missingFields
        };
    }

    private static string CreateMessage(
        Dictionary<string, string?> formPatch,
        List<string> missingFields)
    {
        if (formPatch.Count == 0)
        {
            return "Form için yeni bir bilgi alınamadı.";
        }

        if (missingFields.Count == 0)
        {
            return "Verdiğiniz bilgileri forma aktardım. Tüm gerekli alanlar dolduruldu.";
        }

        var updatedFields =
            formPatch.Keys
                .Select(GetFieldName)
                .ToList();

        var missingFieldNames =
            missingFields
                .Select(GetFieldName)
                .ToList();

        var updatedText =
            string.Join(", ", updatedFields);

        var missingText =
            string.Join(", ", missingFieldNames);

        return
            $"{updatedText} bilgilerini forma aktardım. " +
            $"Eksik bilgiler: {missingText}.";
    }

    private static string GetFieldName(
        string field)
    {
        return field switch
        {
            "firstName" => "ad",
            "lastName" => "soyad",
            "tcNo" => "TC kimlik numarası",
            "email" => "e-posta",
            "motherName" => "anne adı",
            "fatherName" => "baba adı",
            "birthDate" => "doğum tarihi",
            _ => field
        };
    }
}