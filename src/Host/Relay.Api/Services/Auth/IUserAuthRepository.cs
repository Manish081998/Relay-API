namespace Relay.Api.Services.Auth;

public interface IUserAuthRepository
{
    Task<UserAuthStatus?> GetAuthStatusAsync(string globalId, CancellationToken ct = default);
    Task<UserRecord?> UpsertUserAsync(AdUserDetails details, CancellationToken ct = default);
    Task<UserBrandInfo?> GetUserBrandInfoAsync(string globalId, CancellationToken ct = default);
    Task<bool> DeleteUserAsync(string globalId,string createdBy, CancellationToken ct = default);
}
