using System.Xml.Linq;
using Relay.Intranet.Application.Abstractions;
using Relay.Intranet.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Commands.UpdateOrderSection;

public sealed class UpdateOrderSectionCommandHandler : ICommandHandler<UpdateOrderSectionCommand, bool>
{
    private readonly IEdgeOrderRepository _edgeOrders;
    private readonly IStagingFileWriter _stagingWriter;

    public UpdateOrderSectionCommandHandler(IEdgeOrderRepository edgeOrders, IStagingFileWriter stagingWriter)
    {
        _edgeOrders = edgeOrders ?? throw new ArgumentNullException(nameof(edgeOrders));
        _stagingWriter = stagingWriter ?? throw new ArgumentNullException(nameof(stagingWriter));
    }

    public async Task<Result<bool>> HandleAsync(
        UpdateOrderSectionCommand command, CancellationToken cancellationToken = default)
    {
        // Validate state/country before acquiring the lock (read-only, no race concern)
        //if (command.SectionName.Equals("ShipTo", StringComparison.OrdinalIgnoreCase) &&
        //    command.Fields.TryGetValue("State", out var state) &&
        //    command.Fields.TryGetValue("Country", out var country))
        //{
        //    var isValid = await _edgeOrders.IsValidStateForCountryAsync(state, country, cancellationToken);
        //    if (!isValid)
        //        throw new InvalidOperationException($"State '{state}' is not valid for country '{country}'.");
        //}

        string updatedXml;
        XElement root;
        string FileName = $"{command.RepPo}_{command.OrderGuid}.xml";

        string xml = await _stagingWriter.ReadAsync(FileName, cancellationToken);
        var doc = XDocument.Parse(xml);
        root = doc.Root?.Elements().FirstOrDefault() ?? doc.Root!;
        // Remove any misplaced Shipping sub-sections written directly under root by a prior bug
        root.Element("ShippingMethod")?.Remove();
        root.Element("ShippingCharges")?.Remove();
        // Apply field changes to the XML document
        if (command.SectionName.Equals("BrandAccount", StringComparison.OrdinalIgnoreCase))
        {
            ApplyBrandAccountFields(root, command.Fields);
        }
        else if (command.SectionName.Equals("SoldTo", StringComparison.OrdinalIgnoreCase) ||
                 command.SectionName.Equals("ShipTo", StringComparison.OrdinalIgnoreCase))
        {
            var addrEl = GetOrCreateSection(root, "Address");
            var sectionEl = GetOrCreateSection(addrEl, command.SectionName);
            foreach (var (field, value) in command.Fields)
                SetElement(sectionEl, MapAddressField(command.SectionName, field), value);
        }
        else if (command.SectionName.Equals("Shipping", StringComparison.OrdinalIgnoreCase))
        {
            // UI sends all shipping fields combined under "Shipping" — route each field
            // to the correct sub-section based on which element it belongs to.
            var shippingEl = GetOrCreateSection(root, "Shipping");
            var methodEl = GetOrCreateSection(shippingEl, "ShippingMethod");
            var chargesEl = GetOrCreateSection(shippingEl, "ShippingCharges");

            var chargesFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "CommentsToFactory", "CustomerServiceRequest", "MadeinUSA" };

            foreach (var (field, value) in command.Fields)
            {
                var targetEl = chargesFields.Contains(field) ? chargesEl : methodEl;
                SetElement(targetEl, field, value);
            }
        }
        else if (command.SectionName.Equals("ShippingMethod", StringComparison.OrdinalIgnoreCase) ||
                 command.SectionName.Equals("ShippingCharges", StringComparison.OrdinalIgnoreCase))
        {
            var shippingEl = GetOrCreateSection(root, "Shipping");
            var sectionEl = GetOrCreateSection(shippingEl, command.SectionName);
            foreach (var (field, value) in command.Fields)
                SetElement(sectionEl, field, value);
        }
        else
        {
            var sectionEl = GetOrCreateSection(root, command.SectionName);
            foreach (var (field, value) in command.Fields)
                SetElement(sectionEl, field, value);
        }
        updatedXml = doc.ToString();
        // Save updated file on the file location
        await _stagingWriter.WriteAsync(FileName, updatedXml, cancellationToken);

        // Extract only the modified section XML to pass to TrackOrderChanges
        var sectionXml = command.SectionName.ToLower() switch
        {
            "soldto" => root.Element("Address")?.Element("SoldTo")?.ToString(),
            "shipto" => root.Element("Address")?.Element("ShipTo")?.ToString(),
            "shippingmethod" => root.Element("Shipping")?.Element("ShippingMethod")?.ToString(),
            "shippingcharges" => root.Element("Shipping")?.Element("ShippingCharges")?.ToString(),
            "brandaccount" => root.Element("Brand")?.ToString(),
            "quantityinfo" => root.Element("QuantityInfo")?.ToString(),
            "specialinfo" => root.Element("SpecialInfo")?.ToString(),
            "specialitems" => root.Element("SpecialItems")?.ToString(),
            "pricingtotals" => root.Element("PricingTotals")?.ToString(),
            "marketingprogram" => root.Element("MarketingProgram")?.ToString(),
            "orderinfo" => root.Element("OrderInfo")?.ToString(),
            _ => root.Element(command.SectionName)?.ToString()
        } ?? string.Empty;

        await _edgeOrders.TrackOrderChangesAsync(command.OrderGuid, command.RepPo, command.UserId, sectionXml, command.SectionName, updatedXml, cancellationToken);
        return Result.Success(true);
    }

    private static void ApplyBrandAccountFields(XElement root, Dictionary<string, string> fields)
    {
        var brand = GetOrCreateSection(root, "Brand");
        var accountInfo = GetOrCreateSection(brand, "AccountInfo");

        var accountInfoFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "RepAccountNo", "Phone", "Fax" };

        foreach (var (field, value) in fields)
        {
            if (accountInfoFields.Contains(field))
            {
                SetElement(accountInfo, field, value);
            }
            else if (field.Equals("SellingWarehouse", StringComparison.OrdinalIgnoreCase))
            {
                SetElement(brand, "SellingWareHouse", value);
            }
        }
    }
    private static XElement GetOrCreateSection(XElement parent, string sectionName)
    {
        var el = parent.Elements()
            .FirstOrDefault(e => e.Name.LocalName.Equals(sectionName, StringComparison.OrdinalIgnoreCase));
        if (el is not null) return el;
        var newEl = new XElement(sectionName);
        parent.Add(newEl);
        return newEl;
    }

    private static void SetElement(XElement parent, string name, string value)
    {
        var el = parent.Elements()
            .FirstOrDefault(e => e.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (el is not null) el.Value = value;
        else parent.Add(new XElement(name, value));
    }
    private static string MapAddressField(string section, string field) => field switch
    {
        "Name" => "Name1",
        "Address1" => "Street1",
        "Address2" when section.Equals("ShipTo", StringComparison.OrdinalIgnoreCase) => "careof",
        "Address2" => "Street2",
        "Phone" => "Attention",
        _ => field
    };
}
