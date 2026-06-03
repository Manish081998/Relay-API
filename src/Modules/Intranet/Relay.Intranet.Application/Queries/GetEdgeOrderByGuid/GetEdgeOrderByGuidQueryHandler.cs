using Relay.Intranet.Application.Abstractions;
using Relay.Intranet.Application.Mappers;
using Relay.Intranet.Contracts.Dtos;
using Relay.Intranet.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Queries.GetEdgeOrderByGuid;

internal sealed class GetEdgeOrderByGuidQueryHandler : IQueryHandler<GetEdgeOrderByGuidQuery, EdgeOrderDetailDto?>
{
    private readonly IEdgeOrderRepository _orders;
    private readonly IStagingFileWriter _stagingWriter;

    public GetEdgeOrderByGuidQueryHandler(IEdgeOrderRepository orders, IStagingFileWriter stagingWriter)
    {
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
        _stagingWriter = stagingWriter ?? throw new ArgumentNullException(nameof(stagingWriter));
    }

    public async Task<Result<EdgeOrderDetailDto?>> HandleAsync(
        GetEdgeOrderByGuidQuery query, CancellationToken cancellationToken = default)
    {
        var fileName = string.Empty;
        var detail = await _orders.GetByOrderGuidAsync(
            query.OrderGuid, query.RepPo, cancellationToken);

        if (detail?.ErrorMessage is not null)
            return Result.Failure<EdgeOrderDetailDto?>(
                new AppError("Order.SpError", detail.ErrorMessage));

        if (detail?.FinalEdiOrderXml is not null)
        {
            fileName = $"{query.RepPo}_{query.OrderGuid}.xml";
            await _stagingWriter.WriteAsync(fileName, detail.FinalEdiOrderXml, cancellationToken);
            //return Result.Success<EdgeOrderDetailDto?>(detail.ToDto() with { FileName = fileName });
        }

        // Track that this user has the PO open
        await _orders.TrackUserPOAsync(query.UserId, detail.Brand, query.RepPo, fileName);
        var ediStatus = await _orders.GetEdiStatusAsync(detail.RepPoNumber);
        var submitStatus = await _orders.GetEdiSubmitStatusAsync(query.OrderGuid, query.RepPo);

        string? marshalFileLabel = null;
        if (submitStatus != null &&
            (!string.IsNullOrWhiteSpace(submitStatus.UserId) || !string.IsNullOrWhiteSpace(submitStatus.UpdatedTime)))
        {
            marshalFileLabel = $"Created Type Link File On {submitStatus.UpdatedTime} by User {submitStatus.UserId}";
        }
        return Result.Success<EdgeOrderDetailDto?>(detail.ToDto(
            statusText:       ediStatus.FirstOrDefault()?.Status,
            marshalFileLabel: marshalFileLabel));
    }
}
