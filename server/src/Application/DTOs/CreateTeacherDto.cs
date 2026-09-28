using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record CreateTeacherDto : CreateUserDto
{
    [Required(ErrorMessage = "Öğretmen branş / uzmanlık alanı zorunludur.")]
    [StringLength(100, ErrorMessage = "Branş en fazla 100 karakter olabilir.")]
    public string Branch { get; init; } = default!;
}
