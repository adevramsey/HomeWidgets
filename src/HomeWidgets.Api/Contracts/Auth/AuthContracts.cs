using System.ComponentModel.DataAnnotations;

namespace HomeWidgets.Api.Contracts.Auth;

/// <summary>
/// Request body for user registration.
/// </summary>
public record RegisterRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public required string Password { get; init; }

    [MaxLength(100)]
    public string? DisplayName { get; init; }
}

/// <summary>
/// Request body for user login.
/// </summary>
public record LoginRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}

/// <summary>
/// Response after successful authentication.
/// </summary>
public record AuthResponse
{
    public required string Token { get; init; }
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
}

