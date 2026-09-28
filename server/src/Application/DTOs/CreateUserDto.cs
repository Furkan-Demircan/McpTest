using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

// Alan etiketi, sırası, ipucu ve validasyon mesajları formun tek kaynağıdır:
// sunucu bunlardan uygulama manifest'ini üretir (API/Forms/AppManifestBuilder).
public record CreateUserDto
{
    [Display(Name = "Ad", Order = 10)]
    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    [StringLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir.")]
    public string FirstName { get; init; } = default!;

    [Display(Name = "Soyad", Order = 20)]
    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    [StringLength(100, ErrorMessage = "Soyad en fazla 100 karakter olabilir.")]
    public string LastName { get; init; } = default!;

    [Display(Name = "TC Kimlik Numarası", Order = 30, Description = "Sadece rakam, 11 hane.")]
    [Required(ErrorMessage = "TC Kimlik Numarası zorunludur.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik Numarası 11 haneli rakamlardan oluşmalıdır.")]
    public string TcNo { get; init; } = default!;

    [Display(Name = "E-posta Adresi", Order = 40)]
    [Required(ErrorMessage = "E-posta alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; init; } = default!;

    [Display(Name = "Anne Adı", Order = 60)]
    [Required(ErrorMessage = "Anne adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Anne adı en fazla 100 karakter olabilir.")]
    public string MotherName { get; init; } = default!;

    [Display(Name = "Baba Adı", Order = 70)]
    [Required(ErrorMessage = "Baba adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Baba adı en fazla 100 karakter olabilir.")]
    public string FatherName { get; init; } = default!;

    [Display(Name = "Doğum Tarihi", Order = 80, Description = "Takvimden seçilir; YYYY-MM-DD.")]
    [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
    public DateOnly BirthDate { get; init; }
}
