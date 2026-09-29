using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record CreateUserDto : IValidatableObject
{
    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    [StringLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir.")]
    public string FirstName { get; init; } = default!;

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    [StringLength(100, ErrorMessage = "Soyad en fazla 100 karakter olabilir.")]
    public string LastName { get; init; } = default!;

    [Required(ErrorMessage = "TC Kimlik Numarası zorunludur.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik Numarası 11 haneli rakamlardan oluşmalıdır.")]
    public string TcNo { get; init; } = default!;

    [Required(ErrorMessage = "E-posta alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; init; } = default!;

    [Required(ErrorMessage = "Anne adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Anne adı en fazla 100 karakter olabilir.")]
    public string MotherName { get; init; } = default!;

    [Required(ErrorMessage = "Baba adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Baba adı en fazla 100 karakter olabilir.")]
    public string FatherName { get; init; } = default!;

    [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
    public DateOnly BirthDate { get; init; }

    // Kayıt türüne göre makul yaş aralığı; öğretmen DTO'su kendi aralığıyla ezer.
    protected virtual int MinAge => 5;
    protected virtual int MaxAge => 25;
    protected virtual string AgeRangeMessage =>
        $"Öğrenci yaşı {MinAge} ile {MaxAge} arasında olmalıdır; doğum tarihini kontrol edin.";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (BirthDate == default)
        {
            yield break; // [Required] zaten raporlar
        }

        var age = AgeOn(BirthDate, DateOnly.FromDateTime(DateTime.Today));
        if (age < MinAge || age > MaxAge)
        {
            yield return new ValidationResult(AgeRangeMessage, [nameof(BirthDate)]);
        }
    }

    private static int AgeOn(DateOnly birthDate, DateOnly today)
    {
        var age = today.Year - birthDate.Year;
        return birthDate > today.AddYears(-age) ? age - 1 : age;
    }
}
