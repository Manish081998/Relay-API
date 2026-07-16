using Relay.Intranet.Domain.Aggregates;
using Relay.Intranet.Domain.Repositories;
using Relay.Intranet.Infrastructure.Persistence.DataModels;
using Relay.Intranet.Infrastructure.Persistence.SqlQueries;
using Relay.Infrastructure.Core.Data;

namespace Relay.Intranet.Infrastructure.Persistence.Repositories;

internal sealed class CountryRepository : ICountryRepository
{
    private readonly IDbExecutor _db;

    public CountryRepository(IDbExecutor db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task<IReadOnlyList<Countries>> GetAllAsync(string brand, CancellationToken cancellationToken = default) =>
        _db.QueryAsync(
            brand,
            CountryQueries.GetAll,
            r => CountryDataModel.FromRecord(r).ToAggregate(),
            cancellationToken: cancellationToken);
}
