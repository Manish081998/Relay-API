using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Relay.Api.Services.Auth;
using Relay.Api.Settings;

namespace Relay.Api.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register IOptions<JwtSettings> so TokenService can inject it
        services.Configure<JwtSettings>(configuration.GetSection("JsonWebTokenKeys"));

        var jwt = configuration.GetSection("JsonWebTokenKeys").Get<JwtSettings>()
                  ?? throw new InvalidOperationException("JsonWebTokenKeys section is missing.");

        if (string.IsNullOrWhiteSpace(jwt.IssuerSigningKey) || jwt.IssuerSigningKey.Length < 32)
            throw new InvalidOperationException(
                "IssuerSigningKey must be at least 32 characters. " +
                "Store it in user-secrets or Azure KeyVault — never in source control.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.IssuerSigningKey));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = jwt.ValidateIssuerSigningKey,
                IssuerSigningKey = key,
                ValidateIssuer = jwt.ValidateIssuer,
                ValidIssuer = jwt.ValidIssuer,
                ValidateAudience = jwt.ValidateAudience,
                ValidAudience = jwt.ValidAudience,
                RequireExpirationTime = jwt.RequireExpirationTime,
                ValidateLifetime = jwt.ValidateLifetime,
                ClockSkew = TimeSpan.Zero   // no tolerance — token expires exactly on time
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = ctx =>
                {
                    if (ctx.Exception is SecurityTokenExpiredException)
                        ctx.Response.Headers.Append("Token-Expired", "true");
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization(options =>
        {
            // Role-based policies
            options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
            options.AddPolicy("DeveloperOrAbove", p => p.RequireRole("Admin", "Developer"));
            options.AddPolicy("UserOrAbove", p => p.RequireRole("Admin", "Developer", "User"));

            // Claim-based policy example
            options.AddPolicy("ActiveEmployee", p => p.RequireClaim("employment_status", "active"));
        });

        // Register TokenService
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }

    /// <summary>Adds Bearer token security definition to Swagger UI.</summary>
    public static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token. Example: eyJhbGci..."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
