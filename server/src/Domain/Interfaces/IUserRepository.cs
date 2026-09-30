using Domain.Entities;

namespace Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByTcNoAsync(string tcNo, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> SearchByNameAsync(IReadOnlyList<string> terms, int limit, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<User> Items, int Total)> GetPageAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task<bool> ExistsByTcNoAsync(string tcNo, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
