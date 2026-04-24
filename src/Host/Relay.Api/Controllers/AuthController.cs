using System.DirectoryServices.AccountManagement;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Relay.Api.Models.Auth;
using Relay.Api.Services.Auth;
using Relay.Api.Settings;
using static Relay.Api.Routes.ApiRoutes;

namespace Relay.Api.Controllers;

[ApiController]

public sealed class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly RelaySettings _settings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ITokenService tokenService,IOptions<RelaySettings> settings,ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>POST /api/authenticateUser — returns JWT access token + refresh token.</summary>
    [HttpPost(Authenticatication.GenerateToken)]
    [AllowAnonymous]
    public IActionResult GenerateToken([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var bypass = _settings.AppIdentitySettings.AppSettings.BypassLogin;
        var domain = _settings.AppIdentitySettings.ActiveDirectoryConfiguration.Domain;

        if (!bypass)
        {
            try
            {
                using var ctx = new PrincipalContext(ContextType.Domain, domain);
                if (!ctx.ValidateCredentials(dto.UserName, dto.Password))
                {
                    _logger.LogWarning("Failed login attempt for user {UserName}", dto.UserName);
                    return Unauthorized(new { message = "Invalid credentials." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("AD validation error for {UserName}: {Message}", dto.UserName, ex.Message);
                return StatusCode(503, new { message = "Authentication service unavailable." });
            }
        }

        // TODO: load user roles from DB here; hardcoded for initial setup
        var roles = new[] { "User" };

        var tokenResponse = _tokenService.GenerateAccessToken(dto.UserName, dto.UserName, roles);

        // TODO: persist hashed refresh token to DB (see RefreshTokens schema below)
        // var hash = HashToken(tokenResponse.RefreshToken!);
        // await _refreshTokenRepo.StoreAsync(dto.UserName, hash, _settings.JsonWebTokenKeys.RefreshTokenExpiryDays);

        _logger.LogInformation("User {UserName} authenticated successfully", dto.UserName);
        return Ok(tokenResponse);
    }

    /// <summary>POST /api/refresh — rotate refresh token, return new access + refresh token pair.</summary>
    [HttpPost(Authenticatication.RefreshToken)]
    [AllowAnonymous]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        ClaimsPrincipal? principal;
        try
        {
            principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        }
        catch
        {
            return Unauthorized(new { message = "Invalid access token." });
        }

        var userName = principal.Identity?.Name ?? string.Empty;

        var roles = principal.Claims
                               .Where(c => c.Type == ClaimTypes.Role)
                               .Select(c => c.Value);
        var tokenResponse = _tokenService.GenerateAccessToken(userName, userName, roles);

        // TODO: persist new hashed refresh token to DB
        return Ok(tokenResponse);
    }
}
