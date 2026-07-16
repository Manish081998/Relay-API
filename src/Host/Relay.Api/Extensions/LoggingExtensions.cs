using Relay.CrossCutting.Logging;

namespace Relay.Api.Extensions;

public static class LoggingExtensions
{
    public static IHostBuilder AddLogging(this IHostBuilder host)
    {
        return host.ConfigureLogging((ctx, logging) =>
        {
            logging.ClearProviders();

            var options = ctx.Configuration.GetSection("RelayLogging").Get<RelayFileLoggerOptions>()
                         ?? new RelayFileLoggerOptions();

            // File logging — our own implementation, zero third-party dependencies
            logging.AddProvider(new RelayFileLoggerProvider(options));

            // Console output in Development only — Microsoft built-in, no third-party
            if (ctx.HostingEnvironment.IsDevelopment())
                logging.AddConsole();
        });
    }
}
