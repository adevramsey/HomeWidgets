using HomeWidgets.Api.Contracts.Auth;
using HomeWidgets.Core.Entities;
using HomeWidgets.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HomeWidgets.Api.Controllers;

/// <summary>
/// Handles user authentication (register, login, get current user).
/// </summary>
[ApiController]
[Route("api/[controller]")]  // Route: api/auth
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthController(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Register a new user.
    /// POST api/auth/register
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]  // No authentication required
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,  // Data comes from request body JSON
        CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            return BadRequest(new { message = "Email is already registered." });
        }

        // Hash password before storing (never store plain text!)
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Create user entity
        var user = new User(request.Email, passwordHash, request.DisplayName);

        // Save to database
        await _userRepository.AddAsync(user, cancellationToken);

        // Generate JWT token for immediate login
        var token = _tokenService.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName
        });
    }

    /// <summary>
    /// Login with email and password.
    /// POST api/auth/login
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            // Same error message for security (prevents user enumeration)
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // Verify password against stored hash
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            // Same error message for security
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // Generate JWT token
        var token = _tokenService.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName
        });
    }

    /// <summary>
    /// Get current authenticated user info.
    /// GET api/auth/me
    /// Requires valid JWT token in Authorization header.
    /// </summary>
    [HttpGet("me")]
    [Authorize]  // Requires authentication
    public async Task<ActionResult<AuthResponse>> GetCurrentUser(CancellationToken cancellationToken)
    {
        // Extract user ID from JWT claims (set during token validation)
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");

        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(new AuthResponse
        {
            Token = string.Empty,  // Don't issue new token for /me endpoint
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName
        });
    }
}

