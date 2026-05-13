using System.DirectoryServices.AccountManagement;
using System.Security.Claims;
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
    private readonly IUserLoginService _userLoginService;
    private readonly RelaySettings _settings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ITokenService tokenService, IUserLoginService userLoginService, IOptions<RelaySettings> settings, ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _userLoginService = userLoginService;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>POST /api/authenticateUser — validates AD credentials, authorizes via DB, returns JWT + full user profile.</summary>
    [HttpPost(Authenticatication.GenerateToken)]
    [AllowAnonymous]
    public async Task<IActionResult> GenerateToken([FromBody] LoginDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var bypass = _settings.AppIdentitySettings.AppSettings.BypassLogin;
        var domain = _settings.AppIdentitySettings.ActiveDirectoryConfiguration.Domain;

        // Step 1: Validate credentials against Active Directory
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

        // Step 2: DB authorization check + AD profile fetch
        var (user, errorMessage) = await _userLoginService.AuthorizeAndGetUserAsync(dto.UserName, ct);
        if (user is null)
        {
            _logger.LogWarning("Authorization failed for {UserName}: {Message}", dto.UserName, errorMessage);
            return Unauthorized(new { message = errorMessage ?? "Access denied." });
        }

        // Step 3: Generate JWT using the role resolved from the DB
        var roles = new[] { user.UserType };
        var tokenResponse = _tokenService.GenerateAccessToken(dto.UserName, dto.UserName, roles);

        _logger.LogInformation("User {UserName} authenticated successfully as {UserType}", dto.UserName, user.UserType);

        return Ok(new AuthLoginResponse
        {
            AccessToken = tokenResponse.AccessToken,
            ExpiresAt = tokenResponse.ExpiresAt,
            RefreshToken = tokenResponse.RefreshToken,
            User = user
        });
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
