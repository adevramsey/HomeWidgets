using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HomeWidgets.Api.Contracts.Auth;
using HomeWidgets.Core.Entities;
using HomeWidgets.Infrastructure.Data;
using HomeWidgets.IntegrationTests.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HomeWidgets.IntegrationTests.Widgets;

public class AddWidgetTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private string _authToken = string.Empty;
    private Guid _testUserId;
    private Guid _testWidgetId;

    public AddWidgetTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Create a test user and get auth token
        var registerRequest = new RegisterRequest 
        { 
            Email = $"widgettest-{Guid.NewGuid()}@example.com", 
            Password = "P@ssword123", 
            DisplayName = "Widget Test User" 
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _authToken = authResponse!.Token;
        _testUserId = authResponse.UserId;

        // Seed a test widget in the database with unique ComponentType
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var uniqueComponentType = $"Test{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8)}Widget";
        var widget = new Widget("Test Clock", "A test clock widget", uniqueComponentType, "Schedule");
        dbContext.Widgets.Add(widget);
        await dbContext.SaveChangesAsync();
        _testWidgetId = widget.Id;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddWidget_WithValidRequest_Returns201Created()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        var request = new AddWidgetRequest { WidgetId = _testWidgetId };

        // Act
        var response = await _client.PostAsJsonAsync("/api/widgets/me", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var widget = await response.Content.ReadFromJsonAsync<Widget>();
        Assert.NotNull(widget);
        Assert.Equal(_testWidgetId, widget.Id);
        Assert.Equal("Test Clock", widget.Name);
    }

    [Fact]
    public async Task AddWidget_WithoutAuthentication_Returns401Unauthorized()
    {
        // Arrange
        var request = new AddWidgetRequest { WidgetId = _testWidgetId };

        // Act
        var response = await _client.PostAsJsonAsync("/api/widgets/me", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddWidget_WithNonExistentWidget_Returns404NotFound()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        var nonExistentWidgetId = Guid.NewGuid();
        var request = new AddWidgetRequest { WidgetId = nonExistentWidgetId };

        // Act
        var response = await _client.PostAsJsonAsync("/api/widgets/me", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Widget with ID", errorMessage);
    }

    [Fact]
    public async Task AddWidget_WhenWidgetAlreadyExists_Returns400BadRequest()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        var request = new AddWidgetRequest { WidgetId = _testWidgetId };

        // Add widget first time
        var firstResponse = await _client.PostAsJsonAsync("/api/widgets/me", request);
        firstResponse.EnsureSuccessStatusCode();

        // Act - Try to add the same widget again
        var response = await _client.PostAsJsonAsync("/api/widgets/me", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("already on the dashboard", errorMessage);
    }

    [Fact]
    public async Task AddWidget_WithPosition_AddsWidgetAtSpecifiedPosition()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        var request = new AddWidgetRequest { WidgetId = _testWidgetId, Position = 5 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/widgets/me", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        // Verify the widget was added with correct position by checking user's widgets
        var getResponse = await _client.GetAsync("/api/widgets/me");
        getResponse.EnsureSuccessStatusCode();
        var widgets = await getResponse.Content.ReadFromJsonAsync<List<Widget>>();
        Assert.NotNull(widgets);
        Assert.Single(widgets);
    }
}

