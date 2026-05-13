using System.Data;
using Relay.Documentum.Application.Queries.GetBrandAndQueuesAndMapping;
using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Aggregates;
using Relay.Documentum.Domain.Repositories;
using Relay.Documentum.Infrastructure.Persistence.DataModels;
using Relay.Documentum.Infrastructure.Persistence.SqlQueries;
using Relay.Infrastructure.Core.Data;

namespace Relay.Documentum.Infrastructure.Persistence.Repositories;

internal sealed class BrandRepository : IBrandRepository, IBrandMappingQueries
{
    private const string Module = DocumentumInfrastructureModule.ModuleName;

    private readonly IDbExecutor _db;

    public BrandRepository(IDbExecutor db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<IReadOnlyList<Brand>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _db.QueryAsync(
            Module, BrandQueries.GetAll, BrandDataModel.FromRecord,
            cancellationToken: cancellationToken);

        return rows.Select(r => r.ToDomain()).ToArray();
    }

    public async Task<BrandAndQueuesAndMappingDto> GetBrandAndQueuesAndMappingAsync(
        CancellationToken cancellationToken = default)
    {
        var (brands, brandQueueMappings, userQueueMappings, roles) = await _db.QueryMultipleAsync(
            Module,
            BrandQueries.GetBrandAndQueuesAndMapping,
            BrandDataModel.FromRecord,
            BrandQueueMappingDataModel.FromRecord,
            QueueUserMappingDataModel.FromRecord,
            RoleDataModel.FromRecord,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return new BrandAndQueuesAndMappingDto(
            brands.Select(b => new BrandDto(b.BrandId, b.BrandName)).ToList(),
            brandQueueMappings.Select(m => m.ToDto()).ToList(),
            userQueueMappings.Select(m => m.ToDto()).ToList(),
            roles.Select(r => r.ToDto()).ToList());
    }
}
