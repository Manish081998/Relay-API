namespace Relay.Intranet.Domain.Aggregates;

public sealed record EdgeOrder(
    string? ReleaseNumber,
    string? ReleaseName,
    string? AccountNumber,
    string? Name,
    string? RepPO,
    string? LineItems,
    string? TotalNet,
    string? EmailId,
    string? MarketingProgram,
    DateTime? OrderRecdDate,
    string? XmlMacPacOrder,
    string? Brand,
    string? OrderSource);
