namespace Relay.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IEndpointRouteBuilder MapHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", () => Results.Ok(new { status = "ok", service = "Relay.Api" }));
        return endpoints;
    }
}
