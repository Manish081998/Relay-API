using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Relay.CrossCutting.Authentication;

/// <summary>
/// Default implementation backed by HttpContext claims.
/// </summary>
public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpCurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
    }

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public string? UserId => Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public string? UserName => Principal?.Identity?.Name;
    public string? Email => Principal?.FindFirst(ClaimTypes.Email)?.Value;
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyCollection<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray()
        ?? Array.Empty<string>();

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;
}
