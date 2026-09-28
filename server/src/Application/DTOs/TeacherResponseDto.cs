namespace Application.DTOs;

public record TeacherResponseDto : UserResponseDto
{
    // Bilinen kısıt: branş kalıcı değil, yalnızca kayıt cevabında geri döner.
    public string Branch { get; init; } = default!;

    public static TeacherResponseDto From(UserResponseDto user, string branch) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        TcNo = user.TcNo,
        Email = user.Email,
        Branch = branch,
        MotherName = user.MotherName,
        FatherName = user.FatherName,
        BirthDate = user.BirthDate,
        CreatedAt = user.CreatedAt
    };
}
