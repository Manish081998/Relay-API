using System.ComponentModel.DataAnnotations;

namespace Relay.Api.Models.Auth;

public sealed class RefreshTokenRequest
{
    [Required] public string AccessToken  { get; set; } = string.Empty;
    [Required] public string RefreshToken { get; set; } = string.Empty;
}
