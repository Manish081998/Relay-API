using System.Data;
using System.Xml.Linq;
using Relay.Intranet.Domain.Aggregates;

namespace Relay.Intranet.Infrastructure.Persistence.DataModels;

internal sealed class EdgeOrderDetailDataModel
{
    public string? OrderGuid { get; init; }
    public string? FinalEdiOrderXml { get; init; }
    public bool IsFastTrack { get; init; }
    public bool IsEngTrack { get; init; }
    public string? Brand { get; init; }

    private static readonly HashSet<string> KnownModelConfigFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "Line", "PlantCode", "Qty", "Model", "Comment", "Tag",
        "Multiplier", "IndividualPrice", "TotalCost", "SecPlantCode"
    };

    public static EdgeOrderDetailDataModel FromRecord(IDataRecord record) => new()
    {
        OrderGuid = GetString(record, "OrderGUID"),
        FinalEdiOrderXml = GetString(record, "FinalEDIorder"),
        IsFastTrack = GetBool(record, "FastTrack"),
        IsEngTrack = GetBool(record, "EngTrack"),
        Brand = GetString(record, "brand"),
    };

    public EdgeOrderDetail? ToAggregate()
    {
        if (string.IsNullOrWhiteSpace(FinalEdiOrderXml))
            return null;

        XElement? tts;
        try
        {
            tts = XDocument.Parse(FinalEdiOrderXml).Root?.Element("TTS");
        }
        catch
        {
            // SP returned an error message string instead of XML — surface it to the caller.
            return new EdgeOrderDetail(
                OrderGuid: OrderGuid,
                RepPoNumber: null, Brand: null, FileName: null, TrackType: null,
                Info: null, OrderInfo: null, Address: null, BrandAccount: null,
                Shipping: null, MarketingProgram: null,
                LineItemFamilies: Array.Empty<EdgeOrderDetailLineItemFamily>(),
                PricingTotals: new Dictionary<string, string?>(),
                QuantityInfo: new Dictionary<string, string?>(),
                SpecialInfo: new Dictionary<string, string?>(),
                SpecialItems: null, IsFastTrack: false, IsLocked: false,
                ErrorMessage: FinalEdiOrderXml.Trim());
        }

        var orderInfoEl = tts?.Element("OrderInfo");
        var brandEl = tts?.Element("Brand");
        var accountInfoEl = brandEl?.Element("AccountInfo");
        var soldToEl = tts?.Element("Address")?.Element("SoldTo");
        var shipToEl = tts?.Element("Address")?.Element("ShipTo");
        var shippingEl = tts?.Element("Shipping");
        var chargesEl = shippingEl?.Element("ShippingCharges");

        var repPoNo = GetValue(orderInfoEl, "RepPONo");
        var madeInUsa = ParseYesNo(GetValue(chargesEl, "MadeinUSA"));
        var trackType = IsFastTrack ? "FastTrack" : IsEngTrack ? "EngTrack" : string.Empty;


        return new EdgeOrderDetail(
            OrderGuid: OrderGuid,
            RepPoNumber: repPoNo,
            Brand: Brand,
            FileName: string.IsNullOrWhiteSpace(repPoNo) ? $"{OrderGuid}.xml" : $"{repPoNo}.xml",
            TrackType: trackType,
            Info: ParseInfo(orderInfoEl, accountInfoEl),
            OrderInfo: ParseOrderInfo(orderInfoEl),
            Address: ParseAddress(soldToEl, shipToEl),
            BrandAccount: ParseBrandAccount(accountInfoEl, brandEl, Brand),
            Shipping: ParseShipping(shippingEl),
            MarketingProgram: ParseMarketingProgram(tts?.Element("MarketingProgram")),
            LineItemFamilies: ParseLineItems(tts?.Element("LineItems"), madeInUsa),
            PricingTotals: ParseDictionary(tts?.Element("PricingTotals")),
            QuantityInfo: ParseDictionary(tts?.Element("QuantityInfo")),
            SpecialInfo: ParseDictionary(tts?.Element("SpecialInfo")),
            SpecialItems: ParseSpecialItems(tts?.Element("SpecialItems")),
            IsFastTrack: IsFastTrack,
            IsLocked: false,
            FinalEdiOrderXml: FinalEdiOrderXml);
    }

    private static EdgeOrderDetailInfo ParseInfo(XElement? orderInfoEl, XElement? accountInfoEl) => new(
        RepPoNo: GetValue(orderInfoEl, "RepPONo"),
        OrderDate: GetValue(orderInfoEl, "OrderDate"),
        FaxEmail: GetValue(accountInfoEl, "Fax"));

    private static EdgeOrderDetailOrderInfo ParseOrderInfo(XElement? orderInfoEl) => new(
        OrderDate: GetValue(orderInfoEl, "OrderDate"),
        RepPoNo: GetValue(orderInfoEl, "RepPONo"),
        CustomerPoNo: GetValue(orderInfoEl, "CustomerPONo"),
        CustAccountNo: GetValue(orderInfoEl, "CustAccountNo"),
        JobName: GetValue(orderInfoEl, "JobName"),
        SalesPerson: GetValue(orderInfoEl, "SalesPerson"),
        JobGuid: GetValue(orderInfoEl, "JobGuid"));

    private static EdgeOrderDetailAddress ParseAddress(XElement? soldToEl, XElement? shipToEl) => new(
        SoldTo: new EdgeOrderDetailSoldTo(
            Name: GetValue(soldToEl, "Name1"),
            Address1: GetValue(soldToEl, "Street1"),
            Address2: GetValue(soldToEl, "Street2"),
            City: GetValue(soldToEl, "City"),
            State: GetValue(soldToEl, "State"),
            Zip: GetValue(soldToEl, "Zip"),
            Country: GetValue(soldToEl, "country")),
        ShipTo: new EdgeOrderDetailShipTo(
            Name: GetValue(shipToEl, "Name1"),
            Address1: GetValue(shipToEl, "Street1"),
            Address2: GetValue(shipToEl, "careof"),
            City: GetValue(shipToEl, "City"),
            State: GetValue(shipToEl, "State"),
            Zip: GetValue(shipToEl, "Zip"),
            Country: GetValue(shipToEl, "Country"),
            Phone: GetValue(shipToEl, "Phone")));

    private static EdgeOrderDetailBrandAccount ParseBrandAccount(
        XElement? accountInfoEl, XElement? brandEl, string? brandCode) => new(
        RepAccountNo: GetValue(accountInfoEl, "RepAccountNo"),
        Phone: GetValue(accountInfoEl, "Phone"),
        Fax: GetValue(accountInfoEl, "Fax"),
        SellingWarehouse: GetValue(brandEl, "SellingWareHouse"),
        BrandCode: brandCode);

    private static EdgeOrderDetailShipping ParseShipping(XElement? shippingEl)
    {
        var methodEl = shippingEl?.Element("ShippingMethod");
        var chargesEl = shippingEl?.Element("ShippingCharges");

        return new EdgeOrderDetailShipping(
            Method: new EdgeOrderDetailShippingMethod(
                ShipVia: GetValue(methodEl, "ShipVia"),
                NoPartial: GetValue(methodEl, "NoPartial"),
                ShipTerms: GetValue(methodEl, "ShipTerms"),
                CallBeforeDelivery: GetValue(methodEl, "CallBeforeDelivery"),
                Terms: GetValue(methodEl, "Terms"),
                MarkOrder: GetValue(methodEl, "MarkOrder"),
                ShippingInstructions: GetValue(methodEl, "ShippingInstructions")),
            Charges: new EdgeOrderDetailShippingCharges(
                MadeInUsa: ParseYesNo(GetValue(chargesEl, "MadeinUSA")),
                CommentsToFactory: GetValue(chargesEl, "CommentsToFactory"),
                CustomerServiceRequest: GetValue(chargesEl, "CustomerServiceRequest")));
    }

    private static EdgeOrderDetailMarketingProgram ParseMarketingProgram(XElement? el) => new(
        ProgramCode: GetValue(el, "ProgramCode"),
        Program: GetValue(el, "Program"),
        SecureSda: el?.Element("SecureSDA")?.Value?.Trim());

    private static IReadOnlyList<EdgeOrderDetailLineItemFamily> ParseLineItems(
        XElement? lineItemsEl, bool globalMadeInUsa)
    {
        if (lineItemsEl is null)
            return Array.Empty<EdgeOrderDetailLineItemFamily>();

        // Group line items by model name to form families
        var itemsByModel = new Dictionary<string, List<EdgeOrderDetailLineItem>>(StringComparer.OrdinalIgnoreCase);
        var modelOrder = new List<string>();
        var groupIndex = 0;

        foreach (var group in lineItemsEl.Elements("Group"))
        {
            groupIndex++;
            var config = group.Element("ModelConfig");
            if (config is null)
                continue;

            var model = GetValue(config, "Model") ?? $"Group{groupIndex}";
            var line = GetValue(config, "Line");

            var extraFields = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            //// Check for duplicate extrac fields.
            foreach (var e in config.Elements().Where(e => !KnownModelConfigFields.Contains(e.Name.LocalName)))
                extraFields[e.Name.LocalName] = e.Value?.Trim() is { Length: > 0 } v ? v : null;

            var item = new EdgeOrderDetailLineItem(
                Line: line,
                Model: model,
                PlantCode: GetValue(config, "PlantCode"),
                SecondaryPlantCode: GetValue(config, "SecPlantCode"),
                Quantity: GetValue(config, "Qty"),
                IndividualPrice: GetValue(config, "IndividualPrice"),
                TotalCost: GetValue(config, "TotalCost"),
                Comment: GetValue(config, "Comment"),
                Tag: GetValue(config, "Tag"),
                Multiplier: GetValue(config, "Multiplier"),
                GroupId: line ?? groupIndex.ToString(),
                MadeInUsa: globalMadeInUsa,
                ExtraFields: extraFields);

            if (!itemsByModel.ContainsKey(model))
            {
                itemsByModel[model] = new List<EdgeOrderDetailLineItem>();
                modelOrder.Add(model);
            }
            itemsByModel[model].Add(item);
        }

        return modelOrder
            .Select(m => new EdgeOrderDetailLineItemFamily(FamilyTag: m, Items: itemsByModel[m]))
            .ToList();
    }

    private static EdgeOrderDetailSpecialItems? ParseSpecialItems(XElement? el)
    {
        if (el is null) return null;
        return new EdgeOrderDetailSpecialItems(
            IsSpecial: GetValue(el, "IsSpecial"),
            XLines: el.Element("XLines")?.Value?.Trim(),
            CommLines: el.Element("CommLines")?.Value?.Trim(),
            CtrlQty: GetValue(el, "CtrlQty"),
            FMALines: GetValue(el, "FMALines"));
    }

    private static IReadOnlyDictionary<string, string?> ParseDictionary(XElement? el)
    {
        if (el is null)
            return new Dictionary<string, string?>();

        return el.Elements().ToDictionary(
            e => e.Name.LocalName,
            e => e.Value?.Trim() is { Length: > 0 } v ? v : (string?)null);
    }

    private static string? GetValue(XElement? parent, string elementName) =>
        parent?.Element(elementName)?.Value?.Trim() is { Length: > 0 } v ? v : null;

    private static bool ParseYesNo(string? value) =>
        string.Equals(value, "Yes", StringComparison.OrdinalIgnoreCase);

    private static string? GetString(IDataRecord record, string column)
    {
        var ordinal = record.GetOrdinal(column);
        return record.IsDBNull(ordinal) ? null : record.GetValue(ordinal)?.ToString();
    }

    private static bool GetBool(IDataRecord record, string column)
    {
        var ordinal = record.GetOrdinal(column);
        if (record.IsDBNull(ordinal)) return false;
        return record.GetValue(ordinal) switch
        {
            bool b => b,
            int i => i != 0,
            short s => s != 0,
            byte by => by != 0,
            _ => false
        };
    }
}
