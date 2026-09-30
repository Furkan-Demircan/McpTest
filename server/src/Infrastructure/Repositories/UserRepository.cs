using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByTcNoAsync(string tcNo, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.TcNo == tcNo, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    // Her terim adda veya soyadda geçmeli (büyük/küçük harf duyarsız)
    public async Task<IReadOnlyList<User>> SearchByNameAsync(IReadOnlyList<string> terms, int limit, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();

        foreach (var term in terms)
        {
            var pattern = $"%{term}%";
            query = query.Where(u => EF.Functions.ILike(u.FirstName, pattern) || EF.Functions.ILike(u.LastName, pattern));
        }

        return await query
            .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<User> Items, int Total)> GetPageAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var total = await _context.Users.CountAsync(cancellationToken);
        var items = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<bool> ExistsByTcNoAsync(string tcNo, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.TcNo == tcNo, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
