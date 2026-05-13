namespace Relay.Documentum.Infrastructure.Persistence.SqlQueries;

internal static class EdgeOrderQueries
{
    public const string Search = "dbo.usp_SearchEdgeOrders";
    public const string GetDistinctBrands = "SELECT DISTINCT BrandName FROM dbo.BrandMaster WHERE BrandName IS NOT NULL AND BrandName <> '' AND IsActive = 1 ORDER BY BrandName";
    public const string GetProductTypes = "SELECT ProductTypeName FROM dbo.ProductTypeMaster WHERE IsActive = 1 ORDER BY ProductTypeName";
    public const string GetQueuesByBrand = @"
        SELECT q.QueueName
        FROM dbo.QueueMaster q
        INNER JOIN dbo.BrandQueueMapping bqm ON bqm.QueueId = q.QueueId
        INNER JOIN dbo.BrandMaster b ON b.BrandId = bqm.BrandId
        WHERE b.BrandName = @BrandName
          AND bqm.IsActive = 1
          AND q.IsActive = 1
        ORDER BY q.QueueName";
}
