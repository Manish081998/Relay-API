using Relay.Intranet.Domain.Aggregates;

namespace Relay.Intranet.Domain.Repositories;

public interface ICountryRepository
{
    Task<IReadOnlyList<Countries>> GetAllAsync(string brand, CancellationToken cancellationToken = default);
}
