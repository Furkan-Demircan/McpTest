namespace Application.AI.Form;

public class StudentFormValidator
{
    public List<string> GetMissingFields(
        Dictionary<string, string?> formData)
    {
        var missingFields = new List<string>();

        if (string.IsNullOrWhiteSpace(formData.GetValueOrDefault("ad")))
            missingFields.Add("firstName");

        if (string.IsNullOrWhiteSpace(formData.GetValueOrDefault("soyad")))
            missingFields.Add("lastName");

        if (string.IsNullOrWhiteSpace(formData.GetValueOrDefault("tcNo")))
            missingFields.Add("tcNo");

        if (string.IsNullOrWhiteSpace(formData.GetValueOrDefault("email")))
            missingFields.Add("email");

        if (string.IsNullOrWhiteSpace(formData.GetValueOrDefault("anneAdi")))
            missingFields.Add("motherName");

        if (string.IsNullOrWhiteSpace(formData.GetValueOrDefault("babaAdi")))
            missingFields.Add("fatherName");

        if (string.IsNullOrWhiteSpace(formData.GetValueOrDefault("dogumTarihi")))
            missingFields.Add("birthDate");

        return missingFields;
    }
}