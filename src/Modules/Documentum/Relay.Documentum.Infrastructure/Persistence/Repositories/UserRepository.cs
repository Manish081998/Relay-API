using System.Data;
using Relay.Documentum.Domain.Aggregates;
using Relay.Documentum.Domain.Repositories;
using Relay.Documentum.Infrastructure.Persistence.DataModels;
using Relay.Documentum.Infrastructure.Persistence.SqlQueries;
using Relay.Infrastructure.Core.Data;

namespace Relay.Documentum.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private const string Module = DocumentumInfrastructureModule.ModuleName;

    private readonly IDbExecutor _db;

    public UserRepository(IDbExecutor db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _db.QueryAsync(
            Module, UserQueries.GetAll, UserDataModel.FromRecord,
            cancellationToken: cancellationToken);

        return rows.Select(r => r.ToDomain()).ToArray();
    }

    public async Task<int> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var id = await _db.ExecuteScalarAsync<int>(Module, UserQueries.Insert, new
        {
            user.GlobalId,
            user.FirstName,
            user.LastName,
            user.EmailId,
            user.BrandId,
            user.IsActive,
            user.CreatedBy,
        }, cancellationToken: cancellationToken);

        return id > 0 ? id : throw new InvalidOperationException("INSERT did not return a new UserId.");
    }

    public Task<int> UpdateAsync(User user, CancellationToken cancellationToken = default) =>
        _db.ExecuteAsync(Module, UserQueries.Update, new
        {
            user.UserId,
            user.BrandId,
            user.IsActive,
            user.ModifiedBy,
        }, cancellationToken: cancellationToken);
}
