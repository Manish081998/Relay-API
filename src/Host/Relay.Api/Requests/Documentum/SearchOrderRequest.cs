namespace Relay.Api.Requests.Documentum;

public sealed record SearchOrderRequest(
    string? SalesOrderNumber = null,
    string? RepPO = null,
    string? AccountNumber = null,
    string? ProductType = null,
    string? Region = null,
    string? Priority = null,
    string? Brand = null,
    DateTime? CaptureDateFrom = null,
    DateTime? CaptureDateTo = null,
    string? JobName = null,
    string? QueueName = null,
    string? PackageOwner = null,
    int PageNumber = 1,
    int PageSize = 50);
