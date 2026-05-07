namespace Relay.Documentum.Contracts.Dtos;

public sealed record EdgeOrderDto(
    string OrderGuid,
    int OrderSeq,
    string? Brand,
    string? RepPO,
    string? AccountNumber,
    DateTime? OrderDate,
    string? RepCustomer,
    string? RepSalesPerson,
    string? JobNumber,
    string? Status,
    string TotalNet,
    DateTime? OrderRecdDate);
