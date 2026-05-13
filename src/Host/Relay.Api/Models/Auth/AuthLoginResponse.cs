namespace Relay.Api.Models.Auth;

public sealed class AuthLoginResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public string? RefreshToken { get; init; }
    public AuthenticatedUserDto User { get; init; } = null!;
}
