namespace Relay.Documentum.Infrastructure.Persistence.SqlQueries;

internal static class BrandQueries
{
    public const string GetAll = @"
        SELECT BrandId, BrandName
        FROM BrandMaster";

    public const string GetBrandAndQueuesAndMapping = "dbo.usp_GetBrandAndQueuesAndMapping";
}
