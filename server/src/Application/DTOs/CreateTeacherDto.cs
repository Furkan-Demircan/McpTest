using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record CreateTeacherDto : CreateUserDto
{
    [Required(ErrorMessage = "Öğretmen branş / uzmanlık alanı zorunludur.")]
    [StringLength(100, ErrorMessage = "Branş en fazla 100 karakter olabilir.")]
    public string Branch { get; init; } = default!;

    protected override int MinAge => 18;
    protected override int MaxAge => 70;
    protected override string AgeRangeMessage =>
        $"Öğretmen yaşı {MinAge} ile {MaxAge} arasında olmalıdır; doğum tarihini kontrol edin.";
}
