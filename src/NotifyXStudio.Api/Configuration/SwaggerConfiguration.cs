using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace NotifyXStudio.Api.Configuration;

/// <summary>
/// Swagger/OpenAPI configuration for API documentation
/// </summary>
public static class SwaggerConfiguration
{
    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "NotifyX Studio API",
                Description = "A comprehensive project management and workflow automation platform",
                Contact = new OpenApiContact
                {
                    Name = "NotifyX Studio Team",
                    Email = "support@notifyx.studio",
                    Url = new Uri("https://notifyx.studio")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Add security definition for JWT
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
            });

            // Add security requirement
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Include XML comments for better documentation
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Configure schema generation
            options.SchemaFilter<EnumSchemaFilter>();
            options.OperationFilter<ResponseHeadersOperationFilter>();
            options.DocumentFilter<HealthCheckDocumentFilter>();

            // Group by controller tags
            options.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
            options.DocInclusionPredicate((name, api) => true);

            // Custom operation IDs
            options.CustomOperationIds(apiDesc =>
            {
                var controllerName = apiDesc.ActionDescriptor.RouteValues["controller"];
                var actionName = apiDesc.ActionDescriptor.RouteValues["action"];
                return $"{controllerName}_{actionName}";
            });

            // Enable annotations
            options.EnableAnnotations();
        });

        services.AddSwaggerGenNewtonsoftSupport();
    }

    public static void ConfigureSwaggerUI(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
        {
            app.UseSwagger(options =>
            {
                options.RouteTemplate = "api/docs/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/api/docs/v1/swagger.json", "NotifyX Studio API v1");
                options.RoutePrefix = "api/docs";
                options.DocumentTitle = "NotifyX Studio API Documentation";
                
                // Customize UI
                options.DisplayRequestDuration();
                options.EnableDeepLinking();
                options.EnableFilter();
                options.ShowExtensions();
                options.EnableValidator();
                options.SupportedSubmitMethods(Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Get, Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Post, Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Put, Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Delete, Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Patch);

                // Custom CSS
                options.InjectStylesheet("/swagger-ui/custom.css");
                
                // OAuth configuration (if needed)
                options.OAuthClientId("notifyx-swagger");
                options.OAuthAppName("NotifyX Studio API");
                options.OAuthUsePkce();
            });

            // Serve custom CSS
            app.UseStaticFiles();
        }
    }
}

/// <summary>
/// Schema filter for better enum documentation
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            var enumNames = Enum.GetNames(context.Type);
            var enumValues = Enum.GetValues(context.Type);

            for (int i = 0; i < enumNames.Length; i++)
            {
                schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString($"{enumNames[i]} ({(int)enumValues.GetValue(i)!})"));
            }
        }
    }
}

/// <summary>
/// Operation filter for response headers
/// </summary>
public class ResponseHeadersOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses == null)
            return;

        foreach (var response in operation.Responses)
        {
            response.Value.Headers ??= new Dictionary<string, OpenApiHeader>();

            if (!response.Value.Headers.ContainsKey("X-Request-ID"))
            {
                response.Value.Headers.Add("X-Request-ID", new OpenApiHeader
                {
                    Description = "Unique identifier for the request",
                    Schema = new OpenApiSchema { Type = "string" }
                });
            }

            if (!response.Value.Headers.ContainsKey("X-Response-Time"))
            {
                response.Value.Headers.Add("X-Response-Time", new OpenApiHeader
                {
                    Description = "Response time in milliseconds",
                    Schema = new OpenApiSchema { Type = "integer" }
                });
            }
        }
    }
}

/// <summary>
/// Document filter to exclude health check endpoints from swagger
/// </summary>
public class HealthCheckDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var healthCheckPaths = swaggerDoc.Paths
            .Where(x => x.Key.StartsWith("/health"))
            .ToList();

        foreach (var path in healthCheckPaths)
        {
            swaggerDoc.Paths.Remove(path.Key);
        }
    }
}

/// <summary>
/// API versioning configuration
/// </summary>
public static class ApiVersioningConfiguration
{
    public static void ConfigureApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new QueryStringApiVersionReader("version"),
                new HeaderApiVersionReader("X-API-Version"),
                new MediaTypeApiVersionReader("ver")
            );
        });

        services.AddVersionedApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });
    }
}

