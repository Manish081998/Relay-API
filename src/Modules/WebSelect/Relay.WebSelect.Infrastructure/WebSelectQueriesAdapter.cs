using Relay.WebTool.Contracts.Dtos;
using Relay.WebTool.Contracts.Queries;
using Relay.WebTool.Domain.Repositories;

namespace Relay.WebTool.Infrastructure;

internal sealed class WebToolQueriesAdapter : IWebToolQueries
{
    private readonly ISelectionRepository _selections;

    public WebToolQueriesAdapter(ISelectionRepository selections)
    {
        _selections = selections ?? throw new ArgumentNullException(nameof(selections));
    }

    public async Task<SelectionDto?> GetByIdAsync(Guid selectionId, CancellationToken cancellationToken = default)
    {
        var selection = await _selections.GetByIdAsync(selectionId, cancellationToken);
        if (selection is null) return null;

        return new SelectionDto(
            selection.Id,
            selection.Title,
            selection.SubmittedBy,
            selection.CreatedAt,
            selection.SubmittedAt,
            selection.Options
                .OrderBy(o => o.DisplayOrder)
                .Select(o => new SelectionOptionDto(o.Id, o.Label, o.Value, o.DisplayOrder))
                .ToArray());
    }
}
