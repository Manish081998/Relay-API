using Microsoft.OpenApi.Models;
using Relay.Api.Settings;
using Relay.CrossCutting.Email;
using Relay.CrossCutting.ExceptionHandling;

namespace Relay.Api.Extensions;

public static class FrameworkExtensions
{
    public static IServiceCollection AddFrameworkServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Single registration covers all host-level config sections.
        // To add a new section: add a property to RelaySettings — nothing else needed here.
        services.Configure<RelaySettings>(configuration);

        var relay = configuration.Get<RelaySettings>() ?? new RelaySettings();

        services.Configure<EmailSettingsOptions>(configuration.GetSection("AppIdentitySettings:EmailSettings"));
        services.AddEmailService();

        services.AddCors(options =>
        {
            options.AddPolicy("RelayPolicy", policy =>
                policy.WithOrigins(relay.Cors.AllowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod());
        });

        services.AddJwtAuthentication(configuration);

        services.AddControllers();
        services.AddEndpointsApiExplorer();

        // Swagger with Bearer token lock icon
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "Project Relay API",
                Version     = "v1",
                Description = "Modular monolith hosting the Intranet, Documentum and WebTool modules."
            });

            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name         = "Authorization",
                Type         = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme       = "Bearer",
                BearerFormat = "JWT",
                In           = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description  = "Enter your JWT token."
            });
            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        services.AddHttpContextAccessor();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
