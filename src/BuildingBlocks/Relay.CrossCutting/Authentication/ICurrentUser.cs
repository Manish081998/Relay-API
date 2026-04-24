namespace Relay.CrossCutting.Authentication;

/// <summary>
/// Ambient information about the caller. Populated by the authentication middleware.
/// </summary>
public interface ICurrentUser
{
    string? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsInRole(string role);
}
