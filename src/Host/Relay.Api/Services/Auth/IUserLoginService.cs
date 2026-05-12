using Relay.Api.Models.Auth;

namespace Relay.Api.Services.Auth;

public interface IUserLoginService
{
    Task<(AuthenticatedUserDto? User, string? ErrorMessage)> AuthorizeAndGetUserAsync(
        string globalId, CancellationToken ct = default);
}
