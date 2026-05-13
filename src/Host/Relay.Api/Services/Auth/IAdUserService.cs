namespace Relay.Api.Services.Auth;

internal interface IAdUserService
{
    Task<AdUserDetails?> GetUserDetailsAsync(string globalId);
    byte[]? GetProfileImage(string globalId);
}
