using Relay.CrossCutting.Logging;

namespace Relay.Api.Settings;

/// <summary>
/// Root settings class — mirrors the top-level structure of appsettings.json.
/// Every config section maps to exactly one property here.
/// Adding a new appsettings section = add one property here. No other wiring needed.
/// </summary>
public sealed class RelaySettings
{
    public CorsSettings Cors { get; set; } = new();
    public JwtSettings JsonWebTokenKeys { get; set; } = new();
    public AppIdentitySettings AppIdentitySettings { get; set; } = new();
    public RelayFileLoggerOptions RelayLogging { get; set; } = new();
    public FileStorageSettings FileStorage { get; set; } = new();
}
