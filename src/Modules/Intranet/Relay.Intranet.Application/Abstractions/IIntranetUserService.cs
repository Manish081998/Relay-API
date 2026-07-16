using Relay.Intranet.Contracts.Dtos;
using Relay.Intranet.Contracts.Queries;

namespace Relay.Intranet.Application.Abstractions;

/// <summary>
/// Internal facade consumed by the host controllers and by the cross-module query adapter.
/// </summary>
public interface IIntranetUserService : IIntranetUserQueries
{
    Task<Guid> CreateAsync(string displayName, string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDto>> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid userId, CancellationToken cancellationToken = default);
}
