namespace Relay.Intranet.Domain.Aggregates;

public sealed record EdiSubmitStatus(
    string?   UserId,
    string? UpdatedTime);
