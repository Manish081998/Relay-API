using System.Security.Claims;
using Relay.Api.Models.Auth;

namespace Relay.Api.Services.Auth;

public interface ITokenService
{
    TokenResponse GenerateAccessToken(string userId, string userName, IEnumerable<string> roles);
    string        GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
}
