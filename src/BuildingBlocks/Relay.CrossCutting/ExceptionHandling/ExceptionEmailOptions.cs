namespace Relay.CrossCutting.ExceptionHandling;

public sealed class EmailSettingsOptions
{
    public string MailServer { get; set; } = string.Empty;
    public int Port { get; set; } = 25;
    public bool EnableSsl { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    /// <summary>Semicolon-separated list of recipient addresses.</summary>
    public string DevTeamEmailID { get; set; } = string.Empty;
    public string IECProjectMgr  { get; set; } = string.Empty;
    /// <summary>Set to false in appsettings to disable error emails without changing code.</summary>
    public bool Enabled { get; set; } = true;
}
