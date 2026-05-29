namespace Relay.Intranet.Domain.Aggregates;

public sealed record EdgeOrderDetail(
    string? OrderGuid,
    string? RepPoNumber,
    string? Brand,
    string? FileName,
    string? TrackType,
    EdgeOrderDetailInfo? Info,
    EdgeOrderDetailOrderInfo? OrderInfo,
    EdgeOrderDetailAddress? Address,
    EdgeOrderDetailBrandAccount? BrandAccount,
    EdgeOrderDetailShipping? Shipping,
    EdgeOrderDetailMarketingProgram? MarketingProgram,
    IReadOnlyList<EdgeOrderDetailLineItemFamily> LineItemFamilies,
    IReadOnlyDictionary<string, string?> PricingTotals,
    IReadOnlyDictionary<string, string?> QuantityInfo,
    IReadOnlyDictionary<string, string?> SpecialInfo,
    EdgeOrderDetailSpecialItems? SpecialItems,
    bool IsFastTrack,
    bool IsLocked,
    string? ErrorMessage = null);

public sealed record EdgeOrderDetailInfo(
    string? RepPoNo,
    string? OrderDate,
    string? FaxEmail);

public sealed record EdgeOrderDetailOrderInfo(
    string? OrderDate,
    string? RepPoNo,
    string? CustomerPoNo,
    string? CustAccountNo,
    string? JobName,
    string? SalesPerson,
    string? JobGuid);

public sealed record EdgeOrderDetailSoldTo(
    string? Name,
    string? Address1,
    string? Address2,
    string? City,
    string? State,
    string? Zip,
    string? Country);

public sealed record EdgeOrderDetailShipTo(
    string? Name,
    string? Address1,
    string? Address2,
    string? City,
    string? State,
    string? Zip,
    string? Country,
    string? Phone);

public sealed record EdgeOrderDetailAddress(
    EdgeOrderDetailSoldTo? SoldTo,
    EdgeOrderDetailShipTo? ShipTo);

public sealed record EdgeOrderDetailBrandAccount(
    string? RepAccountNo,
    string? Phone,
    string? Fax,
    string? SellingWarehouse,
    string? BrandCode);

public sealed record EdgeOrderDetailShippingMethod(
    string? ShipVia,
    string? NoPartial,
    string? ShipTerms);

public sealed record EdgeOrderDetailShippingCharges(
    bool MadeInUsa,
    string? CommentsToFactory,
    string? CustomerServiceRequest);

public sealed record EdgeOrderDetailShipping(
    EdgeOrderDetailShippingMethod? Method,
    EdgeOrderDetailShippingCharges? Charges);

public sealed record EdgeOrderDetailMarketingProgram(
    string? ProgramCode,
    string? Program,
    string? SecureSda);

public sealed record EdgeOrderDetailLineItem(
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

public sealed record EdgeOrderDetailLineItemFamily(
    string? FamilyTag,
    IReadOnlyList<EdgeOrderDetailLineItem> Items);

public sealed record EdgeOrderDetailSpecialItems(
    string? IsSpecial,
    string? XLines,
    string? CommLines,
    string? CtrlQty,
    string? FMALines);
