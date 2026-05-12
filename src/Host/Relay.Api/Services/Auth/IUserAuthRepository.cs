namespace Relay.Api.Services.Auth;

internal interface IUserAuthRepository
{
    Task<UserAuthStatus?> GetAuthStatusAsync(string globalId, CancellationToken ct = default);
    Task<UserRecord?> UpsertUserAsync(AdUserDetails details, CancellationToken ct = default);
}
