using Relay.CrossCutting.ExceptionHandling;

namespace Relay.Api.Settings;

public sealed class AppIdentitySettings
{
    public ActiveDirectorySettings    ActiveDirectoryConfiguration { get; set; } = new();
    public ApplicationSettings        AppSettings                  { get; set; } = new();
    public EmailConfigurationSettings EmailConfiguration           { get; set; } = new();
    public EmailSettingsOptions       EmailSettings                { get; set; } = new();
}

public sealed class ActiveDirectorySettings
{
    public string Domain { get; set; } = string.Empty;
}

public sealed class ApplicationSettings
{
    public string PlantOpsLogPath      { get; set; } = string.Empty;
    public string AppVersion           { get; set; } = string.Empty;
    public string AppVersionDate       { get; set; } = string.Empty;
    public bool   BypassLogin          { get; set; }
    public bool   IsSearchClearToBuild { get; set; }
    public string ProdMachineName      { get; set; } = string.Empty;
}

public sealed class EmailConfigurationSettings
{
    public string EmailFromAddress { get; set; } = string.Empty;
    public string EmailReadTimer   { get; set; } = string.Empty;
    public string EnableSSL        { get; set; } = string.Empty;
    public string MailServer       { get; set; } = string.Empty;
    public string Port             { get; set; } = string.Empty;
}
