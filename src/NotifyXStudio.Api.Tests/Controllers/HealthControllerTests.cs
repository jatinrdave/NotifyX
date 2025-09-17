using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using Xunit;

namespace NotifyXStudio.Api.Tests.Controllers;

/// <summary>
/// Integration tests for health check endpoints
/// </summary>
public class HealthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HealthControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthResponse = JsonSerializer.Deserialize<HealthCheckResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(healthResponse);
        Assert.Equal("Healthy", healthResponse.Status);
    }

    [Fact]
    public async Task DetailedHealthCheck_ReturnsDetailedInformation()
    {
        // Act
        var response = await _client.GetAsync("/health/detailed");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthResponse = JsonSerializer.Deserialize<DetailedHealthCheckResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(healthResponse);
        Assert.Equal("Healthy", healthResponse.Status);
        Assert.NotNull(healthResponse.Checks);
        Assert.True(healthResponse.TotalDuration >= 0);
        Assert.True(healthResponse.Timestamp > DateTime.MinValue);
    }

    [Fact]
    public async Task ReadyHealthCheck_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task LiveHealthCheck_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ApiRoot_ReturnsWelcomeMessage()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var welcomeResponse = JsonSerializer.Deserialize<WelcomeResponse>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(welcomeResponse);
        Assert.Equal("NotifyX Studio API", welcomeResponse.Name);
        Assert.Equal("1.0.0", welcomeResponse.Version);
        Assert.NotNull(welcomeResponse.Environment);
        Assert.True(welcomeResponse.Timestamp > DateTime.MinValue);
    }
}

/// <summary>
/// Unit tests for JWT token service
/// </summary>
public class JwtTokenServiceTests
{
    private readonly JwtTokenService _tokenService;
    private readonly IConfiguration _configuration;

    public JwtTokenServiceTests()
    {
        var configDict = new Dictionary<string, string>
        {
            ["JwtSettings:SecretKey"] = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
            ["JwtSettings:Issuer"] = "NotifyXStudio",
            ["JwtSettings:Audience"] = "NotifyXStudio.Api",
            ["JwtSettings:ExpiryInMinutes"] = "60"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        var logger = new Mock<ILogger<JwtTokenService>>().Object;
        _tokenService = new JwtTokenService(_configuration, logger);
    }

    [Fact]
    public void GenerateToken_WithValidInput_ReturnsValidToken()
    {
        // Arrange
        var userId = "test-user-id";
        var username = "testuser";
        var email = "test@example.com";
        var roles = new[] { "User", "Admin" };

        // Act
        var token = _tokenService.GenerateToken(userId, username, email, roles);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        var principal = _tokenService.ValidateToken(token);
        Assert.NotNull(principal);
        Assert.Equal(userId, principal.FindFirst("sub")?.Value);
        Assert.Equal(username, principal.Identity?.Name);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var result = _tokenService.ValidateToken(invalidToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        // Act
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Assert
        Assert.NotNull(refreshToken);
        Assert.NotEmpty(refreshToken);
    }
}

/// <summary>
/// Response models for testing
/// </summary>
public class HealthCheckResponse
{
    public string Status { get; set; } = string.Empty;
    public double TotalDuration { get; set; }
    public List<HealthCheckEntry> Checks { get; set; } = new();
}

public class DetailedHealthCheckResponse
{
    public string Status { get; set; } = string.Empty;
    public double TotalDuration { get; set; }
    public DateTime Timestamp { get; set; }
    public List<HealthCheckEntry> Checks { get; set; } = new();
}

public class HealthCheckEntry
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double Duration { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, object>? Data { get; set; }
    public string? Exception { get; set; }
    public List<string>? Tags { get; set; }
}

public class WelcomeResponse
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Documentation { get; set; } = string.Empty;
    public string Health { get; set; } = string.Empty;
    public string HealthDetailed { get; set; } = string.Empty;
    public string HealthUI { get; set; } = string.Empty;
}

