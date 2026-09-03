using Domain.Entities;

namespace Application.DTOs;

public record UserResponseDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string TcNo { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string MotherName { get; init; } = default!;
    public string FatherName { get; init; } = default!;
    public DateOnly BirthDate { get; init; }
    public DateTime CreatedAt { get; init; }

    public static UserResponseDto FromEntity(User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        TcNo = user.TcNo,
        Email = user.Email,
        MotherName = user.MotherName,
        FatherName = user.FatherName,
        BirthDate = user.BirthDate,
        CreatedAt = user.CreatedAt
    };
}
