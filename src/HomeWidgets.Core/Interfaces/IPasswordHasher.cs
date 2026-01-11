namespace HomeWidgets.Core.Interfaces;

/// <summary>
/// Service for securely hashing and verifying passwords.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain text password.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <returns>The hashed password.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a plain text password against a hash.
    /// </summary>
    /// <param name="password">The plain text password to verify.</param>
    /// <param name="hashedPassword">The stored hash to verify against.</param>
    /// <returns>True if the password matches the hash.</returns>
    bool VerifyPassword(string password, string hashedPassword);
}

