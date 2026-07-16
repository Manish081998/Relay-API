namespace Relay.Intranet.Infrastructure.Persistence.SqlQueries;

internal static class CountryQueries
{
    public const string GetAll = "SELECT CODE AS code, Country AS name FROM COUNTRY ORDER BY Country";
}
