using System.Net.Http.Json;
using HomeWidgets.Api.Contracts.Auth;
using HomeWidgets.IntegrationTests.TestHost;
using Xunit;

namespace HomeWidgets.IntegrationTests.Auth;

public class RegisterTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RegisterTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_ReturnsToken_AndCreatesUser()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new RegisterRequest { Email = "integ@example.com", Password = "P@ssword123", DisplayName = "Integ User" };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrEmpty(auth!.Token));
        Assert.Equal(request.Email, auth.Email);
    }
}

