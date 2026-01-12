using System.Net;
using System.Net.Http.Json;
using HomeWidgets.Api.Contracts.Auth;
using HomeWidgets.IntegrationTests.TestHost;
using Xunit;

namespace HomeWidgets.IntegrationTests.Auth;

public class LoginTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public LoginTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange: Register a user first
        var client = _factory.CreateClient();
        var email = $"test-{Guid.NewGuid()}@example.com";
        var password = "TestPassword123!";
        
        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = password,
            DisplayName = "Test User"
        };
        await client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Act: Login with same credentials
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password
        };
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert: 200 OK, token present, email matches
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrEmpty(auth!.Token));
        Assert.Equal(email, auth.Email);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange: Register a user
        var client = _factory.CreateClient();
        var email = $"test-{Guid.NewGuid()}@example.com";
        var password = "CorrectPassword123!";
        
        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = password,
            DisplayName = "Test User"
        };
        await client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Act: Login with WRONG password
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = "WrongPassword999!"
        };
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert: 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsUnauthorized()
    {
        // Arrange: Email that was never registered
        var client = _factory.CreateClient();
        
        // Act: Login with non-existent email
        var loginRequest = new LoginRequest
        {
            Email = $"nonexistent-{Guid.NewGuid()}@example.com",
            Password = "SomePassword123!"
        };
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert: 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

