using Relay.Intranet.Contracts.Dtos;

namespace Relay.Intranet.Contracts.Queries;

/// <summary>
/// Cross-module query contract. Other modules call this interface to read intranet users;
/// they never reach into the Intranet Domain or Application directly.
/// </summary>
public interface IIntranetUserQueries
{
    Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
