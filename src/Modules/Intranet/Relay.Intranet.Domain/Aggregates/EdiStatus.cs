namespace Relay.Intranet.Domain.Aggregates;

public sealed record EdiStatus(
    string? PoNumber,
    string? Status,
    string? User,
    DateTime? TimeStamp);
