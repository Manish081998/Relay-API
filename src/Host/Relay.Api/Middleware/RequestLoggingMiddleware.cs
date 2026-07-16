using System.Diagnostics;
using System.Security.Claims;
using Relay.CrossCutting.Correlation;

namespace Relay.Api.Middleware;

/// <summary>
/// Emits one structured log line per request: method, path, status, duration, user, and
/// correlation ID. All fields are included directly in the message — no third-party
/// ambient context libraries required.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationContextAccessor correlation)
    {
        // Skip infrastructure/tooling paths — not real API traffic
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/health")  ||
            context.Request.Path.StartsWithSegments("/_vs")     ||
            context.Request.Path.StartsWithSegments("/_framework"))
        {
            await _next(context);
            return;
        }

        var stopwatch     = Stopwatch.StartNew();
        var correlationId = correlation.Current?.CorrelationId ?? "none";
        var clientIp      = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var userId   = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            var userName = context.User.Identity?.Name                               ?? "anonymous";

            _logger.LogInformation(
                "{Method} {Path}{QueryString} -> {StatusCode} ({ElapsedMs}ms) | " +
                "User={UserName} ({UserId}) | CorrelationId={CorrelationId} | ClientIp={ClientIp}",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                userName,
                userId,
                correlationId,
                clientIp);
        }
    }
}
