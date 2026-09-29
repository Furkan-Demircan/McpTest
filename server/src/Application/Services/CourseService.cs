using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;

    public CourseService(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<CourseResponseDto> CreateCourseAsync(CreateCourseDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Ders kodu benzersizlik kontrolü
        if (await _courseRepository.ExistsByCodeAsync(dto.CourseCode, cancellationToken))
        {
            throw new DomainException($"'{dto.CourseCode.Trim().ToUpperInvariant()}' ders kodu ile kayıtlı bir ders zaten mevcut.");
        }

        // 2. Domain entity oluşturulması (Entity içindeki doğrulamalar çalışır)
        var course = new Course(
            name: dto.CourseName,
            code: dto.CourseCode,
            credits: dto.Credits,
            description: dto.Description
        );

        // 3. Veritabanına ekleme ve kaydetme
        await _courseRepository.AddAsync(course, cancellationToken);
        await _courseRepository.SaveChangesAsync(cancellationToken);

        return CourseResponseDto.FromEntity(course);
    }

    public async Task<CourseResponseDto?> GetCourseByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var course = await _courseRepository.GetByIdAsync(id, cancellationToken);
        return course is null ? null : CourseResponseDto.FromEntity(course);
    }

    public async Task<IReadOnlyList<CourseResponseDto>> GetAllCoursesAsync(CancellationToken cancellationToken = default)
    {
        var courses = await _courseRepository.GetAllAsync(cancellationToken);
        return courses.Select(CourseResponseDto.FromEntity).ToList();
    }
}
