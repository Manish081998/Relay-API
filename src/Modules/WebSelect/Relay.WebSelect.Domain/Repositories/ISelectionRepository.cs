using Relay.WebTool.Domain.Aggregates;

namespace Relay.WebTool.Domain.Repositories;

public interface ISelectionRepository
{
    Task<Selection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
