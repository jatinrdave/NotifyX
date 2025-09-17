using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotifyXStudio.Api.Middleware;
using NotifyXStudio.Api.Configuration;
using NotifyXStudio.Core;
using NotifyXStudio.Connectors;
using NotifyXStudio.Persistence;
using NotifyXStudio.Runtime;
using Serilog;

// Configure logging first
var builder = WebApplication.CreateBuilder(args);
builder.ConfigureLogging();

try
{
    Log.Information("Starting NotifyX Studio API");

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Configure API versioning
    builder.Services.ConfigureApiVersioning();

    // Configure Swagger/OpenAPI documentation
    builder.Services.ConfigureSwagger();

    // Configure authentication and authorization
    builder.Services.ConfigureAuthentication(builder.Configuration);

    // Configure validation
    builder.Services.ConfigureValidation();

    // Configure health checks (enhanced)
    builder.Services.ConfigureHealthChecks(builder.Configuration);

    // Add JWT token service
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

    // Add NotifyX Studio services
    builder.Services.AddNotifyXStudioCore();
    builder.Services.AddNotifyXStudioConnectors();
    builder.Services.AddNotifyXStudioPersistence(builder.Configuration);
    builder.Services.AddNotifyXStudioRuntime();

    // Add middleware services
    builder.Services.AddNotifyXStudioMiddleware(builder.Configuration);

    // Add SignalR
    builder.Services.AddSignalR();

    // Enhanced CORS with production-ready settings
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            }
            else
            {
                policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            }
        });
    });

    // Add response compression
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline with production-ready middleware

    // Global exception handling (should be first)
    app.UseGlobalExceptionHandling();

    // Add Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = LoggingEnrichers.EnrichWithCorrelationId;
    });

    // Configure health check endpoints
    app.ConfigureHealthCheckEndpoints();

    // Configure Swagger UI
    app.ConfigureSwaggerUI();

    // Security and performance middleware
    app.UseHttpsRedirection();
    app.UseResponseCompression();
    app.UseCors("AllowAll");

    // Use NotifyX Studio middleware
    app.UseNotifyXStudioMiddleware();

    // Authentication and authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Map endpoints
    app.MapControllers();
    app.MapHub<Hubs.WorkflowHub>("/workflowhub");

    // Add a production-ready welcome endpoint
    app.MapGet("/", () => new
    {
        Name = "NotifyX Studio API",
        Version = "1.0.0",
        Environment = app.Environment.EnvironmentName,
        Timestamp = DateTime.UtcNow,
        Documentation = "/api/docs",
        Health = "/health",
        HealthDetailed = "/health/detailed",
        HealthUI = "/health-ui"
    });

    Log.Information("NotifyX Studio API started successfully on {Environment}", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}