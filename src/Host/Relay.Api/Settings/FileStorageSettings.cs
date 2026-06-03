namespace Relay.Api.Settings;

/// <summary>
/// Configuration for physical file storage (documents, versions).
/// Maps to the "FileStorage" section in appsettings.json.
/// </summary>
public sealed class FileStorageSettings
{
    /// <summary>
    /// Root folder where all document files are stored.
    /// Example: "C:\Documentum\PO\Files"
    /// </summary>
    public string BasePath { get; set; } = string.Empty;

    /// <summary>
    /// Staging directory for incoming EDI/order files.
    /// Example: "C:\ASC_EDI\"
    /// </summary>
    public string StagingDir { get; set; } = string.Empty;

    /// <summary>
    /// Impersonation credentials for accessing a remote file server.
    /// </summary>
    public ImpersonationSettings Impersonation { get; set; } = new();
}

/// <summary>
/// Credentials for Windows impersonation when writing to a network share / file server.
/// Set Enabled = true and supply Domain/Username/Password when the storage target
/// requires different credentials than the app pool identity.
/// </summary>
public sealed class ImpersonationSettings
{
    public bool Enabled { get; set; }
    public string Domain { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
