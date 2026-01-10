using HomeWidgets.Core.Entities;

namespace HomeWidgets.Core.Interfaces;

/// <summary>
/// Repository interface for User entity operations.
/// Extends generic repository with user-specific queries.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Finds a user by their email address.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email is already registered.
    /// </summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user with their widgets loaded (eager loading).
    /// </summary>
    Task<User?> GetByIdWithWidgetsAsync(Guid id, CancellationToken cancellationToken = default);
}

