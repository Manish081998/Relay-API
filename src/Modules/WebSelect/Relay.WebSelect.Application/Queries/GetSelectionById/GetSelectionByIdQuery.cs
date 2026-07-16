using Relay.SharedKernel.Application;
using Relay.WebTool.Contracts.Dtos;

namespace Relay.WebTool.Application.Queries.GetSelectionById;

public sealed record GetSelectionByIdQuery(Guid SelectionId) : IQuery<SelectionDto?>;