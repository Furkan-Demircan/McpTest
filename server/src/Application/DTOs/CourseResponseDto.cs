using Domain.Entities;

namespace Application.DTOs;

public record CourseResponseDto
{
    public Guid Id { get; init; }
    public string CourseName { get; init; } = default!;
    public string CourseCode { get; init; } = default!;
    public int Credits { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }

    public static CourseResponseDto FromEntity(Course course) => new()
    {
        Id = course.Id,
        CourseName = course.Name,
        CourseCode = course.Code,
        Credits = course.Credits,
        Description = course.Description,
        CreatedAt = course.CreatedAt
    };
}
