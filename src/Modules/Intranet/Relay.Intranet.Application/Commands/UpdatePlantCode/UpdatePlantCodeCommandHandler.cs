using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Relay.Intranet.Application.Abstractions;
using Relay.Intranet.Application.Commands.UpdateOrderSection;
using Relay.Intranet.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Commands.UpdatePlantCode;

public sealed class UpdatePlantCodeCommandHandler : ICommandHandler<UpdatePlantCodeCommand, bool>
{
    private readonly IStagingFileWriter _stagingWriter;
    private readonly ILogger<UpdateOrderSectionCommandHandler> _logger;
    private readonly IEdgeOrderRepository _edgeOrders;

    public UpdatePlantCodeCommandHandler(IEdgeOrderRepository edgeOrders, IStagingFileWriter stagingWriter, ILogger<UpdateOrderSectionCommandHandler> logger)
    {
        _edgeOrders = edgeOrders ?? throw new ArgumentNullException(nameof(edgeOrders));
        _stagingWriter = stagingWriter ?? throw new ArgumentNullException(nameof(stagingWriter));
        _logger = logger;
    }
    public async Task<Result<bool>> HandleAsync(UpdatePlantCodeCommand command, CancellationToken cancellationToken = default)
    {
        string updatedXml;
        string targetElement;
        XElement root;
        string FileName = $"{command.Po}_{command.OrderGuid}.xml";
        // Read the staging XML file to update.
        string xml = await _stagingWriter.ReadAsync(FileName, cancellationToken);
        var doc = XDocument.Parse(xml);
        root = doc.Root?.Elements().FirstOrDefault() ?? doc.Root!;

        var item = root.Descendants()
            .FirstOrDefault(e => e.Element("Line")?.Value == command.LineNumber);

        if (item is null) return Result.Success(false);

        targetElement = command.IsSecondaryPlant ? "SecPlantCode" : "PlantCode";
        SetElement(item, targetElement, command.NewPlantCode);
        updatedXml = doc.ToString();
        // Save updated file on the file location
        await _stagingWriter.WriteAsync(FileName, updatedXml, cancellationToken);
        await _edgeOrders.TrackOrderChangesAsync(command.OrderGuid, command.Po, command.UserId, command.NewPlantCode, targetElement, updatedXml, cancellationToken);
        return Result.Success(true);
    }

    private static void SetElement(XElement parent, string name, string value)
    {
        var el = parent.Element(name);
        if (el is not null) el.Value = value;
        else parent.Add(new XElement(name, value));
    }
}
