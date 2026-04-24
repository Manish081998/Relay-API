using Relay.WebTool.Contracts.Dtos;

namespace Relay.WebTool.Contracts.Queries;

public interface IWebToolQueries
{
    Task<SelectionDto?> GetByIdAsync(Guid selectionId, CancellationToken cancellationToken = default);
}
