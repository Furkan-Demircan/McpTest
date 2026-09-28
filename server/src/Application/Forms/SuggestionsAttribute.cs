namespace Application.Forms;

/// <summary>
/// Alan için seçim önerileri. Serbest girişi kısıtlamaz; UI'da öneri listesi
/// (datalist) olarak gösterilir ve asistana alanın olası değerleri olarak gider.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SuggestionsAttribute(params string[] values) : Attribute
{
    public IReadOnlyList<string> Values { get; } = values;
}
