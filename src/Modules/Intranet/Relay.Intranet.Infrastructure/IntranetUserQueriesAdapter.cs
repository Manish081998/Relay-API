using Relay.Intranet.Contracts.Dtos;
using Relay.Intranet.Contracts.Queries;
using Relay.Intranet.Domain.Aggregates;
using Relay.Intranet.Domain.Repositories;

namespace Relay.Intranet.Infrastructure;

/// <summary>
/// Implements the cross-module query contract. Other modules depend on
/// <see cref="IIntranetUserQueries"/>; this adapter resolves it without
/// leaking the domain type <see cref="User"/>.
/// </summary>
internal sealed class IntranetUserQueriesAdapter : IIntranetUserQueries
{
    private readonly IUserRepository _users;

    public IntranetUserQueriesAdapter(IUserRepository users)
    {
        _users = users ?? throw new ArgumentNullException(nameof(users));
    }

    public async Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        return user is null ? null : ToDto(user);
    }

    public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByEmailAsync(email, cancellationToken);
        return user is null ? null : ToDto(user);
    }

    private static UserDto ToDto(User user) =>
        new(user.Id, user.DisplayName, user.Email.Value, user.IsActive, user.CreatedAt);
}
