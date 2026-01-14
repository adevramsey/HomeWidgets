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

public class GetMyWidgetsTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    // Fields for factory, client, auth token, test user ID
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private string _authToken = string.Empty;
    private Guid _testUserId;
    private Guid _testWidgetIdClock;
    private Guid _testWidgetIdWeather;
    
    // InitializeAsync: Create test user, get auth token
    public GetMyWidgetsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
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
        
        var uniqueComponentType1 = $"Test{Guid.NewGuid().ToString()
            .Replace("-", "").Substring(0, 8)}Widget";
        var uniqueComponentType2 = $"Test{Guid.NewGuid().ToString()
            .Replace("-", "").Substring(0, 8)}Widget";
        var widgetClock = new Widget("Test Clock", 
            "A test clock widget", uniqueComponentType1, "Schedule");
        var widgetWeather = new Widget("Test Weather",
            "A test weather widget", uniqueComponentType2, "Weather");
        dbContext.Widgets.Add(widgetClock);
        dbContext.Widgets.Add(widgetWeather);
        await dbContext.SaveChangesAsync();
        _testWidgetIdClock = widgetClock.Id;
        _testWidgetIdWeather = widgetWeather.Id;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // Test 1: GetMyWidgets_WithNoWidgets_ReturnsEmptyList
    [Fact]
    public async Task GetMyWidgets_WithNoWidgets_ReturnsEmptyList()
    {
        // Arrange
        // Get Authorization - need this for the method since it uses [Authorize]
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _authToken);
        
        // Act
        var response = await _client.GetAsync("/api/widgets/me");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var widgets = await response.Content.ReadFromJsonAsync<List<Widget>>();
        Assert.NotNull(widgets);
        Assert.Empty(widgets);
        
    }
    
    // Test 2: GetMyWidgets_WithMultipleWidgets_ReturnsOrderedByDisplayOrder
    [Fact]
    public async Task GetMyWidgets_WithMultipleWidgets_ReturnsOrderedByDisplayOrder()
    {
        // Arrange
        // Get Authorization - need this for the method since it uses [Authorize]
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _authToken);
        
        // Add widgets in specific order
        var addClockRequest = new AddWidgetRequest
        {
            WidgetId = _testWidgetIdClock, 
            Position = 2
        };
        await _client.PostAsJsonAsync("/api/widgets/me", addClockRequest);
        
        var addWeatherRequest = new AddWidgetRequest
        {
            WidgetId = _testWidgetIdWeather, 
            Position = 0
        };
        await _client.PostAsJsonAsync("/api/widgets/me", addWeatherRequest);
        
        // Act
        var response = await _client.GetAsync("/api/widgets/me");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var widgets = await response.Content.ReadFromJsonAsync<List<Widget>>();
        Assert.NotNull(widgets);
        Assert.Equal(2, widgets.Count);
        Assert.Equal("Test Clock", widgets[1].Name);
        Assert.Equal("Test Weather", widgets[0].Name);
        
    }
    
    // Test 3: GetMyWidgets_WithInactiveWidgets_FiltersThemOut
    [Fact]
    public async Task GetMyWidgets_WithInactiveWidgets_FiltersThemOut()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _authToken);
        // add widget to user's dashboard
        var addClockRequest = new AddWidgetRequest
        {
            WidgetId = _testWidgetIdClock
        };
        await _client.PostAsJsonAsync("/api/widgets/me", addClockRequest);
        var addWeatherRequest = new AddWidgetRequest
        {
            WidgetId = _testWidgetIdWeather
        };
        await _client.PostAsJsonAsync("/api/widgets/me", addWeatherRequest);
        // Deactivate one widget for testing
        await _client.PutAsync($"/api/widgets/me/{_testWidgetIdClock}/deactivate", null);
        
        
        // Act
        var response = await _client.GetAsync("/api/widgets/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var widgets = await response.Content.ReadFromJsonAsync<List<Widget>>();
        Assert.NotNull(widgets);
        Assert.Single(widgets); //only 1 active widget should be returned
        Assert.Equal("Test Weather", widgets[0].Name);

    }
    
    // Test 4: GetMyWidgets_WithoutAuthentication_Returns401Unauthorized
    [Fact]
    public async Task GetMyWidgets_WithoutAuthentication_Returns401Unauthorized()
    {
        // Arrange
        
        // Act
        var response = await _client.GetAsync("/api/widgets/me");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}