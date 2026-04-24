using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Relay.Api.Models.Auth;
using Relay.Api.Settings;

namespace Relay.Api.Services.Auth;

public sealed class TokenService : ITokenService
{
    private readonly JwtSettings _jwt;

    public TokenService(IOptions<JwtSettings> jwt)
    {
        _jwt = jwt.Value;
    }

    public TokenResponse GenerateAccessToken(string userId, string userName, IEnumerable<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.IssuerSigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,        userId),
            new(JwtRegisteredClaimNames.UniqueName, userName),
            new(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var expiry = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwt.ValidIssuer,
            audience: _jwt.ValidAudience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiry,
            signingCredentials: creds);

        return new TokenResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiry,
            RefreshToken = GenerateRefreshToken()
        };
    }

    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Validates an expired access token and returns its claims.
    /// Used during refresh so we can trust the identity inside the expired token
    /// when paired with a valid refresh token from the DB.
    /// </summary>
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.IssuerSigningKey));

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = _jwt.ValidIssuer,
            ValidateAudience = true,
            ValidAudience = _jwt.ValidAudience,
            ValidateLifetime = false   // intentionally skip — token IS expired during refresh
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(accessToken, parameters, out var validatedToken);

        if (validatedToken is not JwtSecurityToken jwt ||
            !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
            throw new SecurityTokenException("Invalid token algorithm.");

        return principal;
    }
}
