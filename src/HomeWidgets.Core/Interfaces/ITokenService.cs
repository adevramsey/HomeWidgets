using HomeWidgets.Core.Entities;

namespace HomeWidgets.Core.Interfaces;

/// <summary>
/// Service for generating and validating JWT tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT token for a user.
    /// </summary>
    /// <param name="user">The user to generate a token for.</param>
    /// <returns>The JWT token string.</returns>
    string GenerateToken(User user);
}

