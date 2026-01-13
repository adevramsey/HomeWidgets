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

public class RemoveMyWidgetsTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private string _authToken = string.Empty;
    private Guid _testUserId;
    private Guid _testWidgetId;

    public RemoveMyWidgetsTests(CustomWebApplicationFactory factory)
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
        
        var uniqueComponentType = $"Test{Guid.NewGuid().ToString()
            .Replace("-", "").Substring(0, 8)}Widget";
        var testWidget = new Widget(
            "Test Clock",
            "A test clock widget", 
            uniqueComponentType, 
            "Schedule");
        dbContext.Widgets.Add(testWidget);
        await dbContext.SaveChangesAsync();
        _testWidgetId = testWidget.Id;
    }
    
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task RemoveMyWidget_WithValidRequest_RemovesWidgetFromUserDashboard()
    {
        // Arrange
        // Get Authorization - need this for the method since it uses [Authorize]
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        // Add widget to user's dashboard
        var addRequest = new AddWidgetRequest { WidgetId = _testWidgetId };
        var addResponse = await _client.PostAsJsonAsync("/api/widgets/me", addRequest);
        addResponse.EnsureSuccessStatusCode();
        
        // Act
        var response = await _client.DeleteAsync($"/api/widgets/me/{_testWidgetId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync("/api/Widgets/me");
        var widgets = await getResponse.Content.ReadFromJsonAsync<List<Widget>>();
        Assert.NotNull(widgets);
        Assert.Empty(widgets);
    }
    
    [Fact]
    public async Task RemoveMyWidget_WithoutAuthentication_Returns401Unauthorized()
    {
        // Arrange
        // No Authorization header set
        
        // Act
        var response = await _client.DeleteAsync($"/api/widgets/me/{_testWidgetId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RemoveWidget_WidgetMissing_Returns404NotFound()
    {
        // Arrange
        // Get Authorization - need this for the method since it uses [Authorize]
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        var nonExistentWidgetId = Guid.NewGuid();
        
        // Act
        var response = await _client.DeleteAsync($"/api/widgets/me/{nonExistentWidgetId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveWidget_WidgetNotOnDashboard_Returns400BadRequest()
    {
        // Arrange
        // Get Authorization - need this for the method since it uses [Authorize]
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        
        // Act
        // Widget exists in test DB but was NOT added to user's dashboard
        var response = await _client.DeleteAsync($"/api/widgets/me/{_testWidgetId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

    