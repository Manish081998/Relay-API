namespace Relay.Api.Services.Auth;

public interface IAdUserService
{
    Task<AdUserDetails?> GetUserDetailsAsync(string globalId);
    byte[]? GetProfileImage(string globalId);
}
