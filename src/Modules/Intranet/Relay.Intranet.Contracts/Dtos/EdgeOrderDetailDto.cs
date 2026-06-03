namespace Relay.Intranet.Contracts.Dtos;

public sealed record EdgeOrderDetailDto(
    string? OrderGuid,
    string? RepPoNumber,
    string? Brand,
    string? FileName,
    string? TrackType,
    EdgeOrderDetailInfoDto? Info,
    EdgeOrderDetailOrderInfoDto? OrderInfo,
    EdgeOrderDetailAddressDto? Address,
    EdgeOrderDetailBrandAccountDto? BrandAccount,
    EdgeOrderDetailShippingDto? Shipping,
    EdgeOrderDetailMarketingProgramDto? MarketingProgram,
    IReadOnlyList<EdgeOrderDetailLineItemFamilyDto> LineItemFamilies,
    IReadOnlyDictionary<string, string?> PricingTotals,
    IReadOnlyDictionary<string, string?> QuantityInfo,
    IReadOnlyDictionary<string, string?> SpecialInfo,
    EdgeOrderDetailSpecialItemsDto? SpecialItems,
    string? StatusText,
    string? MarshalFileLabel,
    bool IsFastTrack,
    bool IsLocked);

public sealed record EdgeOrderDetailInfoDto(
    string? RepPoNo,
    string? OrderDate,
    string? FaxEmail);

public sealed record EdgeOrderDetailOrderInfoDto(
    string? OrderDate,
    string? RepPoNo,
    string? CustomerPoNo,
    string? CustAccountNo,
    string? JobName,
    string? SalesPerson,
    string? JobGuid);

public sealed record EdgeOrderDetailSoldToDto(
    string? Name,
    string? Address1,
    string? Address2,
    string? City,
    string? State,
    string? Zip,
    string? Country);

public sealed record EdgeOrderDetailShipToDto(
    string? Name,
    string? Address1,
    string? Address2,
    string? City,
    string? State,
    string? Zip,
    string? Country,
    string? Phone);

public sealed record EdgeOrderDetailAddressDto(
    EdgeOrderDetailSoldToDto? SoldTo,
    EdgeOrderDetailShipToDto? ShipTo);

public sealed record EdgeOrderDetailBrandAccountDto(
    string? RepAccountNo,
    string? Phone,
    string? Fax,
    string? SellingWarehouse,
    string? BrandCode);

public sealed record EdgeOrderDetailShippingMethodDto(
    string? ShipVia,
    string? NoPartial,
    string? ShipTerms,
    string? CallBeforeDelivery,
    string? Terms,
    string? MarkOrder,
    string? ShippingInstructions);

public sealed record EdgeOrderDetailShippingChargesDto(
    bool MadeInUsa,
    string? CommentsToFactory,
    string? CustomerServiceRequest);

public sealed record EdgeOrderDetailShippingDto(
    EdgeOrderDetailShippingMethodDto? Method,
    EdgeOrderDetailShippingChargesDto? Charges);

public sealed record EdgeOrderDetailMarketingProgramDto(
    string? ProgramCode,
    string? Program,
    string? SecureSda);

public sealed record EdgeOrderDetailLineItemDto(
    string? Line,
    string? Model,
    string? PlantCode,
    string? SecondaryPlantCode,
    string? Quantity,
    string? IndividualPrice,
    string? TotalCost,
    string? Comment,
    string? Tag,
    string? Multiplier,
    string? GroupId,
    bool MadeInUsa,
    IReadOnlyDictionary<string, string?> ExtraFields);

public sealed record EdgeOrderDetailLineItemFamilyDto(
    string? FamilyTag,
    IReadOnlyList<EdgeOrderDetailLineItemDto> Items);

public sealed record EdgeOrderDetailSpecialItemsDto(
    string? IsSpecial,
    string? XLines,
    string? CommLines,
    string? CtrlQty,
    string? FMALines);
