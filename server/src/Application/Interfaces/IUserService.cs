using Application.DTOs;

namespace Application.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<UserResponseDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserResponseDto?> GetUserByTcNoAsync(string tcNo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserResponseDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserResponseDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserResponseDto>> SearchUsersByNameAsync(string name, int limit, CancellationToken cancellationToken = default);
    Task<PagedResultDto<UserResponseDto>> GetUsersPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
