using System.Reflection;

namespace Relay.SharedKernel.Domain;

/// <summary>
/// Smart-enum base. Prefer this to plain enums when behaviour or metadata is
/// attached to discrete values.
/// </summary>
public abstract class Enumeration : IComparable
{
    public int Id { get; }
    public string Name { get; }

    protected Enumeration(int id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        }
        Id = id;
        Name = name;
    }

    public override string ToString() => Name;

    public static IEnumerable<T> GetAll<T>() where T : Enumeration =>
        typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(T))
            .Select(f => f.GetValue(null))
            .OfType<T>();

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration other)
        {
            return false;
        }

        return GetType() == other.GetType() && Id.Equals(other.Id);
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public int CompareTo(object? obj) =>
    obj is Enumeration e
        ? Id.CompareTo(e.Id)
        : throw new ArgumentException("Not an Enumeration.", nameof(obj));
    public static bool operator ==(Enumeration? left, Enumeration? right) => Equals(left, right);
    public static bool operator !=(Enumeration? left, Enumeration? right) => !Equals(left, right);
    public static bool operator <(Enumeration left, Enumeration right) => left.CompareTo(right) < 0;
    public static bool operator >(Enumeration left, Enumeration right) => left.CompareTo(right) > 0;
    public static bool operator <=(Enumeration left, Enumeration right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Enumeration left, Enumeration right) => left.CompareTo(right) >= 0;
}
