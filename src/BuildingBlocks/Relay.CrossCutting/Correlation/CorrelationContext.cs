namespace Relay.CrossCutting.Correlation;

/// <summary>
/// Per-request correlation data. Attached to every log entry and outgoing HTTP/DB call
/// to trace a request end-to-end across modules.
/// </summary>
public sealed class CorrelationContext
{
    public string CorrelationId { get; }
    public string? CausationId { get; }
    public DateTimeOffset StartedAt { get; }

    public CorrelationContext(string correlationId, string? causationId = null)
    {
        CorrelationId = string.IsNullOrWhiteSpace(correlationId)
            ? throw new ArgumentException("CorrelationId cannot be empty.", nameof(correlationId))
            : correlationId;
        CausationId = causationId;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public static CorrelationContext New() => new(Guid.NewGuid().ToString("N"));
}

/// <summary>
/// Access the correlation context for the current request.
/// </summary>
public interface ICorrelationContextAccessor
{
    CorrelationContext? Current { get; set; }
}

/// <summary>
/// AsyncLocal-backed accessor. Populated by correlation middleware.
/// </summary>
public sealed class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<CorrelationContext?> CurrentContext = new();

    public CorrelationContext? Current
    {
        get => CurrentContext.Value;
        set => CurrentContext.Value = value;
    }
}
