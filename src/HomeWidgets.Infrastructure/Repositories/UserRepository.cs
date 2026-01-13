using HomeWidgets.Core.Entities;
using HomeWidgets.Core.Interfaces;
using HomeWidgets.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeWidgets.Infrastructure.Repositories;

/// <summary>
/// User repository implementation with user-specific queries.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="email"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant().Trim();
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant().Trim();
        return await _dbSet
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<User?> GetByIdWithWidgetsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserWidgets)
                .ThenInclude(uw => uw.Widget)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
}

