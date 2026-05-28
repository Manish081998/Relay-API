using System.Runtime.InteropServices;
using Microsoft.Extensions.Options;
using Microsoft.Win32.SafeHandles;
using Relay.Api.Settings;

namespace Relay.Api.Services;

/// <summary>
/// Handles physical file I/O with optional Windows impersonation
/// for remote file server access.
/// </summary>
public interface IFileStorageService
{
    /// <summary>Root storage path from configuration.</summary>
    string BasePath { get; }

    /// <summary>Write a stream to the given absolute file path (creating directories as needed).</summary>
    Task SaveFileAsync(string absolutePath, Stream content, CancellationToken ct = default);

    /// <summary>Open a file for reading and return the stream.</summary>
    FileStream OpenRead(string absolutePath);

    /// <summary>Check whether a file exists at the given absolute path.</summary>
    bool FileExists(string absolutePath);
}

internal sealed class FileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IOptions<FileStorageSettings> settings, ILogger<FileStorageService> logger)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger   = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string BasePath => _settings.BasePath;

    public async Task SaveFileAsync(string absolutePath, Stream content, CancellationToken ct = default)
    {
        var directory = Path.GetDirectoryName(absolutePath)!;

        if (_settings.Impersonation.Enabled)
        {
            _logger.LogInformation("Saving file with impersonation to {Path}", absolutePath);
            await RunImpersonatedAsync(async () =>
            {
                Directory.CreateDirectory(directory);
                await using var fs = new FileStream(absolutePath, FileMode.Create);
                await content.CopyToAsync(fs, ct);
            });
        }
        else
        {
            Directory.CreateDirectory(directory);
            await using var fs = new FileStream(absolutePath, FileMode.Create);
            await content.CopyToAsync(fs, ct);
        }
    }

    public FileStream OpenRead(string absolutePath)
    {
        return new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public bool FileExists(string absolutePath)
    {
        return File.Exists(absolutePath);
    }

    // ── Windows impersonation ──────────────────────────────────────────────

    private async Task RunImpersonatedAsync(Func<Task> action)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _logger.LogWarning("Impersonation requested but not running on Windows. Falling back to app identity.");
            await action();
            return;
        }

        var imp = _settings.Impersonation;

        var success = LogonUser(
            imp.Username, imp.Domain, imp.Password,
            LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_WINNT50, out var tokenHandle);

        if (!success)
        {
            var error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"LogonUser failed for {imp.Domain}\\{imp.Username}. Win32 error: {error}");
        }

        try
        {
            using var identity = new System.Security.Principal.WindowsIdentity(tokenHandle.DangerousGetHandle());
            await System.Security.Principal.WindowsIdentity.RunImpersonatedAsync(
                identity.AccessToken, action);
        }
        finally
        {
            tokenHandle.Dispose();
        }
    }

    // ── P/Invoke declarations ──────────────────────────────────────────────

    private const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
    private const int LOGON32_PROVIDER_WINNT50 = 3;

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool LogonUser(
        string lpszUsername, string lpszDomain, string lpszPassword,
        int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);
}
