using Relay.WebTool.Contracts.Dtos;
using Relay.WebTool.Domain.Aggregates;

namespace Relay.WebTool.Application.Mappers;

internal static class SelectionMappers
{
    public static SelectionDto ToDto(this Selection selection) =>
        new SelectionDto(
            selection.Id,
            selection.Title,
            selection.SubmittedBy,
            selection.CreatedAt,
            selection.SubmittedAt,
            selection.Options.Select(o => new SelectionOptionDto(o.Id, o.Label, o.Value, o.DisplayOrder)).ToList());
}
