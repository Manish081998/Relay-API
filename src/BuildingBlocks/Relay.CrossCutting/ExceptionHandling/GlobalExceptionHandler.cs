using System.Net;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Relay.CrossCutting.Correlation;
using Relay.CrossCutting.Email;

namespace Relay.CrossCutting.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly ICorrelationContextAccessor _correlation;
    private readonly IEmailService _emailService;
    private readonly EmailSettingsOptions _emailOptions;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        ICorrelationContextAccessor correlation,
        IEmailService emailService,
        IOptions<EmailSettingsOptions> emailOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _emailOptions = emailOptions?.Value ?? throw new ArgumentNullException(nameof(emailOptions));
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var userName = httpContext.User.Identity?.Name ?? "anonymous";
        var correlationId = _correlation.Current?.CorrelationId ?? "none";
        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path.ToString();
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var origin = FindOriginFrame(exception);

        _logger.LogError(
            "{Method} {Path} | {ExceptionType}: {ExceptionMessage} | Origin: {Origin} | " +
            "User={UserName} CorrelationId={CorrelationId} ClientIp={ClientIp}",
            method, path,
            exception.GetType().Name, exception.Message,
            origin,
            userName, correlationId, clientIp);

        var problem = MapToProblemDetails(exception);
        var status = problem.Status ?? (int)HttpStatusCode.InternalServerError;

        // Send email only for unhandled server errors — not for validation / 401 / 404
        if (status == (int)HttpStatusCode.InternalServerError)
            _ = SendErrorEmailAsync(exception, method, path, userName, correlationId, clientIp, origin, status);

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private Task SendErrorEmailAsync(
        Exception exception,
        string method, string path,
        string userName, string correlationId, string clientIp, string origin,
        int statusCode)
    {
        if (!_emailOptions.Enabled || string.IsNullOrWhiteSpace(_emailOptions.DevTeamEmailID))
            return Task.CompletedTask;

        var to = _emailOptions.DevTeamEmailID
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var cc = string.IsNullOrWhiteSpace(_emailOptions.IECProjectMgr)
            ? null
            : _emailOptions.IECProjectMgr
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var request = new EmailRequest(
            To: to,
            Subject: $"ProjectRelay Error - {method} {path} — {exception.GetType().Name}",
            Body: BuildEmailBody(exception, method, path, userName, correlationId, clientIp, origin, statusCode),
            Cc: cc);

        return _emailService.SendAsync(request);
    }

    private static string BuildEmailBody(
        Exception exception,
        string method,
        string path,
        string userName,
        string correlationId,
        string clientIp,
        string origin,
        int statusCode)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><body style='font-family:Consolas,monospace;font-size:13px;'>");
        sb.AppendLine("<h2 style='color:#c00;'>Project Relay — Error Report</h2>");
        sb.AppendLine("<table cellpadding='6' cellspacing='0' border='1' style='border-collapse:collapse;'>");

        void Row(string label, string value)
        {
            sb.AppendLine($"<tr><td style='font-weight:bold;background:#f5f5f5;width:160px;'>{label}</td>" +
                          $"<td>{WebUtility.HtmlEncode(value)}</td></tr>");
        }

        Row("Time (UTC)", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        Row("HTTP Method", method);
        Row("Endpoint", path);
        Row("Status Code", statusCode.ToString());
        Row("User", userName);
        Row("Correlation ID", correlationId);
        Row("Client IP", clientIp);
        Row("Exception Type", exception.GetType().FullName ?? exception.GetType().Name);
        Row("Exception Message", exception.Message);
        Row("Origin", origin);

        if (exception.InnerException is not null)
            Row("Inner Exception", exception.InnerException.Message);

        sb.AppendLine("</table>");
        sb.AppendLine("<h3 style='margin-top:20px;'>Stack Trace</h3>");
        sb.AppendLine($"<pre style='background:#f9f9f9;padding:10px;border:1px solid #ddd;'>" +
                      $"{WebUtility.HtmlEncode(exception.StackTrace ?? "n/a")}</pre>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }

    private static string FindOriginFrame(Exception exception)
    {
        var stackTrace = exception.StackTrace;
        if (string.IsNullOrEmpty(stackTrace)) return "unknown";

        foreach (var line in stackTrace.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("at Relay.", StringComparison.Ordinal))
                return trimmed["at ".Length..];
        }

        return stackTrace.Split('\n').FirstOrDefault()?.Trim() ?? "unknown";
    }

    private static ProblemDetails MapToProblemDetails(Exception ex) => ex switch
    {
        ValidationException v => new ProblemDetails
        {
            Status = (int)HttpStatusCode.BadRequest,
            Title = "Validation failed",
            Detail = string.Join("; ", v.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")),
            Type = "https://projectrelay.adti/errors/validation"
        },
        UnauthorizedAccessException => new ProblemDetails
        {
            Status = (int)HttpStatusCode.Unauthorized,
            Title = "Unauthorized",
            Type = "https://projectrelay.adti/errors/unauthorized"
        },
        KeyNotFoundException => new ProblemDetails
        {
            Status = (int)HttpStatusCode.NotFound,
            Title = "Resource not found",
            Type = "https://projectrelay.adti/errors/not-found"
        },
        _ => new ProblemDetails
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "An unexpected error occurred",
            Detail = $"{ex.GetType().Name}: {ex.Message}",
            Type = "https://projectrelay.adti/errors/internal"
        }
    };
}
