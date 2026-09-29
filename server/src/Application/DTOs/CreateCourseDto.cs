using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record CreateCourseDto
{
    [Required(ErrorMessage = "Ders adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Ders adı en fazla 100 karakter olabilir.")]
    public string CourseName { get; init; } = default!;

    [Required(ErrorMessage = "Ders kodu zorunludur.")]
    [RegularExpression(@"^[A-Za-z]{3}\d{3}$", ErrorMessage = "Ders kodu 3 harf ve 3 rakamdan oluşmalıdır (örn. MAT101).")]
    public string CourseCode { get; init; } = default!;

    [Required(ErrorMessage = "Kredi zorunludur.")]
    [Range(1, 10, ErrorMessage = "Kredi 1 ile 10 arasında olmalıdır.")]
    public int Credits { get; init; }

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    public string? Description { get; init; }
}
