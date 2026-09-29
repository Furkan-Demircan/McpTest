using Application.DTOs;

namespace Application.Interfaces;

public interface ICourseService
{
    Task<CourseResponseDto> CreateCourseAsync(CreateCourseDto dto, CancellationToken cancellationToken = default);
    Task<CourseResponseDto?> GetCourseByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CourseResponseDto>> GetAllCoursesAsync(CancellationToken cancellationToken = default);
}
