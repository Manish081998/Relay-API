using System.Text.RegularExpressions;
using Relay.SharedKernel.Domain;

namespace Relay.Intranet.Domain.ValueObjects;

/// <summary>
/// Email address — validated at construction. Value-object equality (case-insensitive).
/// </summary>
public sealed partial class EmailAddress : ValueObject
{
    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(value));
        }
        var normalised = value.Trim().ToLowerInvariant();
        if (!EmailRegex().IsMatch(normalised))
        {
            throw new ArgumentException($"'{value}' is not a valid email address.", nameof(value));
        }
        return new EmailAddress(normalised);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();
}
