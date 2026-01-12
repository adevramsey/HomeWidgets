using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HomeWidgets.Core.Entities;
using HomeWidgets.Core.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HomeWidgets.Infrastructure.Authentication;

/// <summary>
/// JWT token generation service.
/// Creates signed tokens containing user claims.
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    /// <summary>
    /// Generates a JWT token for the given user.
    /// Token contains user ID, email, display name and expiration.
    /// </summary>
    public string GenerateToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        // Claims = data stored in the token payload
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),  // Subject - WHO the token is for
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),  // JWT ID - unique per token
            new("displayName", user.DisplayName)
        };

        // Create signing key from secret
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Build the token
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,      // Who created the token
            audience: _jwtSettings.Audience,   // Who the token is for
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            signingCredentials: credentials);

        // Serialize to string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

