using Relay.Intranet.Contracts.Dtos;
using Relay.Intranet.Domain.Aggregates;

namespace Relay.Intranet.Application.Mappers;

internal static class EdgeOrderDetailMappers
{
    public static EdgeOrderDetailDto ToDto(
        this EdgeOrderDetail detail,
        string? statusText = null,
        string? marshalFileLabel = null,
        IEnumerable<(string Code, string Description)>? plantCodes = null,
        IEnumerable<(string Code, string Description)>? shipTerms = null) => new(
        OrderGuid:        detail.OrderGuid,
        RepPoNumber:      detail.RepPoNumber,
        Brand:            detail.Brand,
        FileName:         detail.FileName,
        TrackType:        detail.TrackType,
        Info:             detail.Info?.ToDto(),
        OrderInfo:        detail.OrderInfo?.ToDto(),
        Address:          detail.Address?.ToDto(),
        BrandAccount:     detail.BrandAccount?.ToDto(),
        Shipping:         detail.Shipping?.ToDto(),
        MarketingProgram: detail.MarketingProgram?.ToDto(),
        LineItemFamilies: detail.LineItemFamilies.Select(f => f.ToDto()).ToList(),
        PricingTotals:    detail.PricingTotals,
        QuantityInfo:     detail.QuantityInfo,
        SpecialInfo:      detail.SpecialInfo,
        SpecialItems:     detail.SpecialItems?.ToDto(),
        StatusText:       statusText,
        MarshalFileLabel: marshalFileLabel,
        IsFastTrack:      detail.IsFastTrack,
        IsLocked:         detail.IsLocked,
        PlantCodes:       plantCodes?.Select(p => new LookupItemDto(p.Code, p.Description)).ToList() ?? [],
        ShipTerms:        shipTerms?.Select(s => new LookupItemDto(s.Code, s.Description)).ToList() ?? []);

    private static EdgeOrderDetailInfoDto ToDto(this EdgeOrderDetailInfo info) =>
        new(info.RepPoNo, info.OrderDate, info.FaxEmail);

    private static EdgeOrderDetailOrderInfoDto ToDto(this EdgeOrderDetailOrderInfo oi) =>
        new(oi.OrderDate, oi.RepPoNo, oi.CustomerPoNo, oi.CustAccountNo,
            oi.JobName, oi.SalesPerson, oi.JobGuid);

    private static EdgeOrderDetailAddressDto ToDto(this EdgeOrderDetailAddress address) =>
        new(address.SoldTo?.ToDto(), address.ShipTo?.ToDto());

    private static EdgeOrderDetailSoldToDto ToDto(this EdgeOrderDetailSoldTo s) =>
        new(s.Name, s.Address1, s.Address2, s.City, s.State, s.Zip, s.Country);

    private static EdgeOrderDetailShipToDto ToDto(this EdgeOrderDetailShipTo s) =>
        new(s.Name, s.Address1, s.Address2, s.City, s.State, s.Zip, s.Country, s.Phone);

    private static EdgeOrderDetailBrandAccountDto ToDto(this EdgeOrderDetailBrandAccount ba) =>
        new(ba.RepAccountNo, ba.Phone, ba.Fax, ba.SellingWarehouse, ba.BrandCode);

    private static EdgeOrderDetailShippingDto ToDto(this EdgeOrderDetailShipping shipping) =>
        new(shipping.Method?.ToDto(), shipping.Charges?.ToDto());

    private static EdgeOrderDetailShippingMethodDto ToDto(this EdgeOrderDetailShippingMethod m) =>
        new(m.ShipVia, m.NoPartial, m.ShipTerms, m.CallBeforeDelivery, m.Terms, m.MarkOrder, m.ShippingInstructions);

    private static EdgeOrderDetailShippingChargesDto ToDto(this EdgeOrderDetailShippingCharges c) =>
        new(c.MadeInUsa, c.CommentsToFactory, c.CustomerServiceRequest);

    private static EdgeOrderDetailMarketingProgramDto ToDto(this EdgeOrderDetailMarketingProgram mp) =>
        new(mp.ProgramCode, mp.Program, mp.SecureSda);

    private static EdgeOrderDetailLineItemFamilyDto ToDto(this EdgeOrderDetailLineItemFamily family) =>
        new(family.FamilyTag, family.Items.Select(i => i.ToDto()).ToList());

    private static EdgeOrderDetailLineItemDto ToDto(this EdgeOrderDetailLineItem item) =>
        new(item.Line, item.Model, item.PlantCode, item.SecondaryPlantCode,
            item.Quantity, item.IndividualPrice, item.TotalCost,
            item.Comment, item.Tag, item.Multiplier, item.GroupId,
            item.MadeInUsa, item.ExtraFields);

    private static EdgeOrderDetailSpecialItemsDto ToDto(this EdgeOrderDetailSpecialItems si) =>
        new(si.IsSpecial, si.XLines, si.CommLines, si.CtrlQty, si.FMALines);
}
