using System.IdentityModel.Tokens.Jwt;
using HomeWidgets.Core.Entities;
using HomeWidgets.Infrastructure.Authentication;
using Microsoft.Extensions.Options;

namespace HomeWidgets.UnitTests.Authentication;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public TokenServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "ThisIsATestSecretKeyThatIsAtLeast32Characters!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationInMinutes = 60
        };

        var options = Options.Create(_jwtSettings);
        _tokenService = new TokenService(options);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsToken()
    {
        // Arrange
        var user = new User("test@example.com", "hashedpassword", "Test User");

        // Act
        var token = _tokenService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsValidJwtFormat()
    {
        // Arrange
        var user = new User("test@example.com", "hashedpassword", "Test User");

        // Act
        var token = _tokenService.GenerateToken(user);

        // Assert
        var parts = token.Split('.');
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public void GenerateToken_ContainsCorrectClaims()
    {
        // Arrange
        var user = new User("test@example.com", "hashedpassword", "Test User");

        // Act
        var token = _tokenService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal(user.Id.ToString(), jwtToken.Subject);
        Assert.Equal(user.Email, jwtToken.Claims.First(c => c.Type == "email").Value);
        Assert.Equal(user.DisplayName, jwtToken.Claims.First(c => c.Type == "displayName").Value);
    }

    [Fact]
    public void GenerateToken_HasCorrectIssuerAndAudience()
    {
        // Arrange
        var user = new User("test@example.com", "hashedpassword", "Test User");

        // Act
        var token = _tokenService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal(_jwtSettings.Issuer, jwtToken.Issuer);
        Assert.Contains(_jwtSettings.Audience, jwtToken.Audiences);
    }

    [Fact]
    public void GenerateToken_HasCorrectExpiration()
    {
        // Arrange
        var user = new User("test@example.com", "hashedpassword", "Test User");
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _tokenService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var expectedExpiration = beforeGeneration.AddMinutes(_jwtSettings.ExpirationInMinutes);
        Assert.True(jwtToken.ValidTo <= expectedExpiration.AddSeconds(5));
        Assert.True(jwtToken.ValidTo >= expectedExpiration.AddSeconds(-5));
    }

    [Fact]
    public void GenerateToken_WithNullUser_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _tokenService.GenerateToken(null!));
    }

    [Fact]
    public void GenerateToken_EachTokenHasUniqueJti()
    {
        // Arrange
        var user = new User("test@example.com", "hashedpassword", "Test User");

        // Act
        var token1 = _tokenService.GenerateToken(user);
        var token2 = _tokenService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken1 = handler.ReadJwtToken(token1);
        var jwtToken2 = handler.ReadJwtToken(token2);

        var jti1 = jwtToken1.Claims.First(c => c.Type == "jti").Value;
        var jti2 = jwtToken2.Claims.First(c => c.Type == "jti").Value;

        Assert.NotEqual(jti1, jti2);
    }
}

