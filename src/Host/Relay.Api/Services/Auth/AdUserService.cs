using System.DirectoryServices;
using Microsoft.Extensions.Options;
using Relay.Api.Settings;

namespace Relay.Api.Services.Auth;

internal sealed class AdUserService : IAdUserService
{
    private readonly string _ldapRoot;

    public AdUserService(IOptions<RelaySettings> settings)
    {
        _ldapRoot = "LDAP://" + settings.Value.AppIdentitySettings.ActiveDirectoryConfiguration.Domain;
    }

    public Task<AdUserDetails?> GetUserDetailsAsync(string globalId) =>
        Task.Run(() =>
        {
            using var root = new DirectoryEntry(_ldapRoot);
            using var search = new DirectorySearcher(root)
            {
                Filter = $"(&(objectCategory=person)(objectClass=user)(SAMAccountname={EscapeLdap(globalId)}))",
                ServerTimeLimit = TimeSpan.FromSeconds(10)
            };

            search.PropertiesToLoad.AddRange(new[]
            {
                "samaccountname", "givenName", "sn", "mail",
                "company", "department", "physicalDeliveryOfficeName", "Title"
            });

            var result = search.FindOne();
            if (result is null) return null;

            var props = result.Properties;

            var samAccount = GetString(props, "samaccountname") ?? globalId;
            var firstName = GetString(props, "givenName") ?? string.Empty;
            var lastName = GetString(props, "sn") ?? string.Empty;

            return new AdUserDetails
            {
                GlobalId = samAccount,
                FirstName= firstName.Trim(),
                LastName= lastName.Trim(),
                EmailId = GetString(props, "mail"),
                CompanyName = GetString(props, "company"),
                Department = GetString(props, "department"),
                Office = GetString(props, "physicalDeliveryOfficeName"),
                Title = GetString(props, "Title")
            };
        });

    public byte[]? GetProfileImage(string globalId)
    {
        using var root = new DirectoryEntry(_ldapRoot);
        using var search = new DirectorySearcher(root)
        {
            Filter = $"(&(objectCategory=person)(objectClass=user)(SAMAccountname={EscapeLdap(globalId)}))"
        };

        search.PropertiesToLoad.AddRange(new[] { "samaccountname", "thumbnailPhoto" });

        var result = search.FindOne();
        if (result is null) return null;

        var props = result.Properties;
        return props.Contains("thumbnailPhoto") && props["thumbnailPhoto"].Count > 0
            ? (byte[])props["thumbnailPhoto"][0]
            : null;
    }

    private static string? GetString(ResultPropertyCollection props, string key) =>
        props.Contains(key) && props[key].Count > 0 ? props[key][0]?.ToString() : null;

    // Prevent LDAP injection by escaping special characters in the filter value.
    private static string EscapeLdap(string value) =>
        value
            .Replace("\\", "\\5c")
            .Replace("*", "\\2a")
            .Replace("(", "\\28")
            .Replace(")", "\\29")
            .Replace("\0", "\\00");
}
