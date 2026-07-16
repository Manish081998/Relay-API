namespace Relay.Intranet.Contracts.Dtos;

public sealed record EdiStatusDto(
    string? PoNumber,
    string? Status,
    string? User,
    DateTime? TimeStamp);
