namespace Application.AI.Form;

public class StudentFormValidator
{
    public List<string> GetMissingFields(
        Dictionary<string, string?> formData)
    {
        var missingFields = new List<string>();

        if (string.IsNullOrWhiteSpace(formData.GetValueOrDefault("firstName")))
            missingFields.Add("firstName");

        if (string.IsNullOrWhiteSpace(formData.GetValueOrDefault("lastName")))
            missingFields.Add("lastName");

        if (string.IsNullOrWhiteSpace(formData.GetValueOrDefault("tcNo")))
            missingFields.Add("tcNo");

        if (string.IsNullOrWhiteSpace(formData.GetValueOrDefault("email")))
            missingFields.Add("email");

        if (string.IsNullOrWhiteSpace(formData.GetValueOrDefault("motherName")))
            missingFields.Add("motherName");

        if (string.IsNullOrWhiteSpace(formData.GetValueOrDefault("fatherName")))
            missingFields.Add("fatherName");

        if (string.IsNullOrWhiteSpace(formData.GetValueOrDefault("birthDate")))
            missingFields.Add("birthDate");

        return missingFields;
    }
}