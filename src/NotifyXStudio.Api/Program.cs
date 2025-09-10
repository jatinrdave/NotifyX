using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotifyXStudio.Api.Middleware;
using NotifyXStudio.Core;
using NotifyXStudio.Connectors;
using NotifyXStudio.Persistence;
using NotifyXStudio.Runtime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add NotifyX Studio services
builder.Services.AddNotifyXStudioCore();
builder.Services.AddNotifyXStudioConnectors();
builder.Services.AddNotifyXStudioPersistence(builder.Configuration);
builder.Services.AddNotifyXStudioRuntime();

// Add middleware services
builder.Services.AddNotifyXStudioMiddleware(builder.Configuration);

// Add SignalR
builder.Services.AddSignalR();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Use NotifyX Studio middleware
app.UseNotifyXStudioMiddleware();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<Hubs.WorkflowHub>("/workflowhub");
app.MapHealthChecks("/health");

app.Run();