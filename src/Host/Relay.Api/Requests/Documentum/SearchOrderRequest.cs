namespace Relay.Api.Requests.Documentum;

public sealed record SearchOrderRequest(
    int? OrderSeq = null,
    string? RepPO = null,
    string? AccountNumber = null,
    string? Brand = null,
    DateTime? OrderDateFrom = null,
    DateTime? OrderDateTo = null,
    int PageNumber = 1,
    int PageSize = 50);
