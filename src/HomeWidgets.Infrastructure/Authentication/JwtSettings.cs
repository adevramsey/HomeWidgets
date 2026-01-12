namespace HomeWidgets.Infrastructure.Authentication;

/// <summary>
/// Strongly-typed configuration for JWT authentication.
/// Maps to "JwtSettings" section in appsettings.json.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// Secret key used to sign JWT tokens.
    /// Must be at least 32 characters for HS256 algorithm.
    /// NEVER store in appsettings.json - use secrets or environment variables.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Issuer claim - identifies who created the token (our API).
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Audience claim - identifies the intended recipient (our App).
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Token validity duration in minutes. After this, user must login again.
    /// </summary>
    public int ExpirationInMinutes { get; set; } = 60;
}

