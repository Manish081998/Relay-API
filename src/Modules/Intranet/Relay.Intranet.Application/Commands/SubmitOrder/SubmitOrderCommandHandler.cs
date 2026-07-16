using System.Text;
using System.Xml.Linq;
using Relay.Intranet.Application.Abstractions;
using Relay.Intranet.Domain.Aggregates;
using Relay.Intranet.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Commands.SubmitOrder;

public sealed class SubmitOrderCommandHandler : ICommandHandler<SubmitOrderCommand, bool>
{
    private readonly IEdgeOrderRepository _edgeOrders;
    private readonly IStagingFileWriter _stagingWriter;
    private const string CxmlHeader =
        "<?xml version=\"1.0\" standalone=\"yes\"?>\r\n" +
        "<cXML version=\"1.0\" payloadID=\"XML PO BORG\">\r\n" +
        "  <Header>\r\n" +
        "    <From>\r\n" +
        "      <Credential domain=\"BORG\">\r\n" +
        "        <Identity />\r\n" +
        "      </Credential>\r\n" +
        "    </From>\r\n" +
        "    <To>\r\n" +
        "      <Credential domain=\"Ruskin Corp\">\r\n" +
        "        <Identity />\r\n" +
        "      </Credential>\r\n" +
        "    </To>\r\n" +
        "    <Sender>\r\n" +
        "      <Credential domain=\"ASC\">\r\n" +
        "        <Identity />\r\n" +
        "        <SharedSecret />\r\n" +
        "      </Credential>\r\n" +
        "    </Sender>\r\n" +
        "  </Header>";
    public SubmitOrderCommandHandler(IEdgeOrderRepository edgeOrders,IStagingFileWriter stagingWriter)
    {
        _edgeOrders = edgeOrders ?? throw new ArgumentNullException(nameof(edgeOrders));
        _stagingWriter = stagingWriter ?? throw new ArgumentNullException(nameof(stagingWriter));
    }
    public async Task<Result<bool>> HandleAsync(SubmitOrderCommand command, CancellationToken cancellationToken = default)
    {
        //Read updated file from staging location
        string FileName = $"{command.Po}_{command.OrderGuid}.xml";
        string xml = await _stagingWriter.ReadAsync(FileName, cancellationToken);
        var doc = XDocument.Parse(xml);
        var root = doc.Root?.Elements().FirstOrDefault() ?? doc.Root!;
        var madeInUsa = root.Descendants("MadeinUSA").FirstOrDefault()?.Value == "Y";
        // Transform XML for AS/400 submission
        var submissionXml = await PrepareForSubmissionAsync(xml, command.Brand, madeInUsa, FileName);
        var workingFileName = command.OrderGuid + ".xml";
        await _stagingWriter.WriteWorkingAsync(workingFileName, submissionXml, cancellationToken);

        ////TODO: Upload via FTP
        
        // Clean up staging file after successful upload
        await _stagingWriter.DeleteFile(FileName, cancellationToken);

        // Dont need to save whole xml again into database. 
        //await _edgeOrders.TrackOrderChangesAsync(command.OrderGuid, command.Po, "jparaste", xml,"Submit", xml, command.Brand, cancellationToken);
        return Result.Success(true);
    }

    public async Task<string> PrepareForSubmissionAsync(string xmlContent, string brand, bool madeInUsa, string fileName)
    {
        ////TODO: 
        var fieldTypes = await _edgeOrders.GetFieldTypesAsync(brand);

        var doc = XDocument.Parse(xmlContent);
        var root = doc.Root?.Elements().FirstOrDefault() ?? doc.Root!;

        // Stamp the actual file name onto the TTS element's file attribute
        root.SetAttributeValue("file", fileName);

        // 1. Apply MadeInUSA plant code substitutions
        if (madeInUsa)
        {
            ApplyMadeInUsaPlants(root);
        }

        // 2. Normalize option nodes — converts unknown ModelConfig fields to <Option> elements,
        //    formats Line/Multiplier, and applies Fraction/NUM CDATA padding per GetFieldType() logic
        NormalizeOptionNodes(root, fieldTypes);

        // 3. Pad/format top-level numeric fields
        FormatNumericFields(root);

        // 4. Apply brand-specific transformations
        ApplyBrandTransformations(root, brand);

        // 5. Wrap in cXML header
        return WrapWithCxmlHeader(doc);
    }

    private static void ApplyMadeInUsaPlants(XElement root)
    {
        foreach (var madeInEl in root.Descendants("MadeinUSA"))
            madeInEl.Value = "Y";
    }

    private static void NormalizeOptionNodes(XElement root, FieldTypeData fieldTypes)
    {
        var lineItems = root.Element("LineItems");
        if (lineItems == null) return;

        foreach (var group in lineItems.Elements())
        {
            foreach (var modelConfig in group.Elements())
            {
                var currentModel = modelConfig.Element("Model")?.Value ?? string.Empty;

                // Pass 1: format known fields; convert unknown fields to <Option> elements
                foreach (var field in modelConfig.Elements().ToList())
                {
                    var name = field.Name.LocalName;

                    if (name.Equals("Line", StringComparison.OrdinalIgnoreCase))
                    {
                        field.Value = field.Value.PadLeft(3, '0');
                        continue;
                    }

                    if (name.Equals("Multiplier", StringComparison.OrdinalIgnoreCase))
                    {
                        var idx = field.Value.IndexOf(':');
                        if (idx >= 0)
                        {
                            var after = field.Value[(idx + 1)..].TrimStart();
                            field.Value = after.Length > 5 ? after[..5] : after;
                        }
                        continue;
                    }

                    if (_optionSkipFields.Contains(name)) continue;

                    var value = field.Value;
                    if (string.IsNullOrWhiteSpace(value)) continue;

                    // SQ# normalisation from SetOptions()
                    value = value
                        .Replace("SQ#50541-EE&VP", "SQ 50541-EE-VP")
                        .Replace("SQ#50541-EE &VP", "SQ 50541-EE-VP")
                        .Replace("SQ#", "SQ ");

                    // Strip leading underscore used to prefix option names starting with a digit
                    var optionName = name.StartsWith('_') ? name[1..] : name;

                    var fieldType = fieldTypes.GetFieldType(name, currentModel);
                    var valueContent = BuildOptionValueContent(value, fieldType);

                    // <Option>FieldName<Value>...</Value></Option>
                    field.AddBeforeSelf(new XElement("Option", optionName, new XElement("Value", valueContent)));
                }

                // Pass 2: remove original fields that were converted to Option elements
                foreach (var field in modelConfig.Elements().ToList())
                {
                    if (!_optionKeepFields.Contains(field.Name.LocalName))
                        field.Remove();
                }
            }
        }
    }

    // Elements preserved after the cleanup pass; everything else is removed
    private static readonly HashSet<string> _optionKeepFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "Line", "ReleaseCode", "Qty", "Model", "CARModel", "Comment", "Tag",
        "Multiplier", "IndividualPrice", "TotalCost", "PlantCode", "Option"
    };

    private static readonly HashSet<string> _optionSkipFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "Line", "ReleaseCode", "EngineeringLineItem", "Qty", "Model", "CARModel",
        "Comment", "Tag", "Multiplier", "IndividualPrice", "TotalCost", "PlantCode",
        "addoninformation", "options"
    };

    private static object BuildOptionValueContent(string value, string fieldType)
    {
        if (fieldType.Equals("Fraction", StringComparison.OrdinalIgnoreCase))
        {
            // PadLeft 9 spaces + 6 zeros, wrapped in CDATA
            // e.g. "72 3/4" → "   72 3/4000000"
            var formatted = value.PadLeft(9, ' ') + "000000";
            return new XCData(formatted);
        }

        if (fieldType.Equals("NUM", StringComparison.OrdinalIgnoreCase))
        {
            // Integer part PadLeft 9 spaces; decimal part PadRight 6 zeros; wrapped in CDATA
            // e.g. "3.5" → "        3500000"  (9+6=15 chars)
            // e.g. ".5" → "        0500000"
            if (value.StartsWith('.'))
                value = "0" + value;

            var dotIdx = value.IndexOf('.');
            string intPart, fracPart;
            if (dotIdx > 0)
            {
                intPart = value[..dotIdx];
                fracPart = value[(dotIdx + 1)..];
            }
            else
            {
                intPart = value;
                fracPart = string.Empty;
            }

            var formatted = intPart.PadLeft(9, ' ') + fracPart.PadRight(6, '0');
            return new XCData(formatted);
        }

        return value;
    }

    private static void FormatNumericFields(XElement root)
    {
        var decimalFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "IndividualPrice", "TotalCost", "FreightAmount", "Multiplier"
        };

        foreach (var el in root.Descendants())
        {
            if (string.IsNullOrWhiteSpace(el.Value)) continue;

            if (el.Name.LocalName.Equals("Qty", StringComparison.OrdinalIgnoreCase))
            {
                if (decimal.TryParse(el.Value, out var qty))
                    el.Value = ((int)qty).ToString();
            }
            else if (decimalFields.Contains(el.Name.LocalName))
            {
                if (decimal.TryParse(el.Value, out var num))
                    el.Value = num.ToString("F2");
            }
        }
    }

    private static void ApplyBrandTransformations(XElement root, string brand)
    {
        if (brand.Equals("Titus", StringComparison.OrdinalIgnoreCase) ||
            brand.Equals("TTS", StringComparison.OrdinalIgnoreCase))
        {
            var commentsEl = root.Element("CommentsToFactory");
            if (commentsEl is not null && string.IsNullOrEmpty(commentsEl.Value))
                commentsEl.Value = "SAMPLE";
        }

        foreach (var el in root.Descendants().Where(e => !e.HasElements))
        {
            el.Value = el.Value
                .Replace("HK$", "HKD")
                .Replace("NT$", "NTD");
        }
    }

    private static string WrapWithCxmlHeader(XDocument orderDoc)
    {
        var sb = new StringBuilder();
        sb.AppendLine(CxmlHeader);
        sb.AppendLine(orderDoc.Root?.ToString() ?? string.Empty);
        sb.AppendLine("</cXML>");
        return sb.ToString();
    }
}
