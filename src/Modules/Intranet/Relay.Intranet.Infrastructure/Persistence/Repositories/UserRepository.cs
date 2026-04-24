using Relay.Infrastructure.Core.Data;
using Relay.Intranet.Domain.Aggregates;
using Relay.Intranet.Domain.Repositories;
using Relay.Intranet.Infrastructure.Persistence.DataModels;
using Relay.Intranet.Infrastructure.Persistence.SqlQueries;

namespace Relay.Intranet.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private const string Module = IntranetInfrastructureModule.ModuleName;

    private readonly IDbExecutor _db;

    public UserRepository(IDbExecutor db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var data = await _db.QuerySingleOrDefaultAsync(
            Module, UserQueries.GetById, UserDataModel.FromRecord,
            new { Id = id }, cancellationToken: cancellationToken);
        return data?.ToAggregate();
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var data = await _db.QuerySingleOrDefaultAsync(
            Module, UserQueries.GetByName, UserDataModel.FromRecord,
            new { Email = email.ToLowerInvariant() }, cancellationToken: cancellationToken);
        return data?.ToAggregate();
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default) =>
        _db.ExecuteAsync(Module, UserQueries.Update, new
        {
            user.Id,
            user.DisplayName,
            Email = user.Email.Value,
            user.IsActive,
            user.DeactivatedAt,
        }, cancellationToken: cancellationToken);
}
