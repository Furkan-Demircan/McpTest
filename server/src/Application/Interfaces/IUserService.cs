using Application.DTOs;

namespace Application.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<UserResponseDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserResponseDto?> GetUserByTcNoAsync(string tcNo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserResponseDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
}
