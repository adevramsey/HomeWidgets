using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HomeWidgets.Api.Contracts.Auth;
using HomeWidgets.IntegrationTests.TestHost;
using Xunit;

namespace HomeWidgets.IntegrationTests.Auth;

public class MeEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public MeEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMe_WithValidToken_ReturnsUserInfo()
    {
        // Arrange: Register user, get token
        var email = $"test-{Guid.NewGuid()}@example.com";
        var password = "TestPassword123!";
        var token = await RegisterAndGetTokenAsync(email, password);
        
        // Act: GET /api/auth/me with Authorization: Bearer {token}
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync("/api/auth/me");

        // Assert: 200 OK, userId/email/displayName match
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.Equal(email, auth!.Email);
        Assert.Equal("Test User", auth.DisplayName);
    }

    [Fact]
    public async Task GetMe_WithoutToken_ReturnsUnauthorized()
    {
        // Act: GET /api/auth/me with NO Authorization header
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/auth/me");

        // Assert: 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithInvalidToken_ReturnsUnauthorized()
    {
        // Act: GET /api/auth/me with Authorization: Bearer fake-invalid-token
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "fake-invalid-token");
        var response = await client.GetAsync("/api/auth/me");

        // Assert: 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // Helper method - YOU CAN USE THIS
    private async Task<string> RegisterAndGetTokenAsync(string email, string password)
    {
        var client = _factory.CreateClient();
        var registerRequest = new RegisterRequest 
        { 
            Email = email, 
            Password = password, 
            DisplayName = "Test User" 
        };
        
        var response = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        response.EnsureSuccessStatusCode();
        
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.Token;
    }
}

