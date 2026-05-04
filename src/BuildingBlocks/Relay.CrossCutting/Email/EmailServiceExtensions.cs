using Microsoft.Extensions.DependencyInjection;

namespace Relay.CrossCutting.Email;

public static class EmailServiceExtensions
{
    /// <summary>
    /// Registers <see cref="IEmailService"/> with the DI container.
    /// Call this once from the host's service-registration entry point.
    /// </summary>
    public static IServiceCollection AddEmailService(this IServiceCollection services)
    {
        services.AddSingleton<IEmailService, SmtpEmailService>();
        return services;
    }
}
