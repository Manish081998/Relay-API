using Relay.SharedKernel.Application;
using Relay.WebTool.Application.Mappers;
using Relay.WebTool.Contracts.Dtos;
using Relay.WebTool.Domain.Repositories;

namespace Relay.WebTool.Application.Queries.GetSelectionById;

public sealed class GetSelectionByIdQueryHandler : IQueryHandler<GetSelectionByIdQuery, SelectionDto?>
{
    private readonly ISelectionRepository _selections;

    public GetSelectionByIdQueryHandler(ISelectionRepository selections)
    {
        _selections = selections ?? throw new ArgumentNullException(nameof(selections));
    }

    public async Task<Result<SelectionDto?>> HandleAsync(GetSelectionByIdQuery query, CancellationToken cancellationToken = default)
    {
        var selection = await _selections.GetByIdAsync(query.SelectionId, cancellationToken);
        return Result.Success(selection?.ToDto());
    }
}