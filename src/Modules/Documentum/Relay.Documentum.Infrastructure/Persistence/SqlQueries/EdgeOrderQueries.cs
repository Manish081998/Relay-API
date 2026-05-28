namespace Relay.Documentum.Infrastructure.Persistence.SqlQueries;

internal static class EdgeOrderQueries
{
    public const string Search = "dbo.usp_SearchEdgeOrders";
    public const string GetDistinctBrands = "SELECT DISTINCT BrandName FROM dbo.BrandMaster WHERE BrandName IS NOT NULL AND BrandName <> '' AND IsActive = 1 ORDER BY BrandName";
    public const string GetProductTypesByBrand = @"
        SELECT pt.ProductTypeId, pt.ProductTypeName
        FROM dbo.ProductTypeMaster pt
        INNER JOIN dbo.ProductTypeBrandMapping ptbm ON ptbm.ProductTypeId = pt.ProductTypeId
        INNER JOIN dbo.BrandMaster b ON b.BrandId = ptbm.BrandId
        WHERE b.BrandName = @BrandName
          AND ptbm.IsActive = 1
          AND pt.IsActive = 1
        ORDER BY pt.ProductTypeName";
    public const string GetRegionsByBrand = @"
        SELECT r.RegionId, r.RegionName
        FROM dbo.RegionMaster r
        INNER JOIN dbo.RegionBrandMapping rbm ON rbm.RegionId = r.RegionId
        INNER JOIN dbo.BrandMaster b ON b.BrandId = rbm.BrandId
        WHERE b.BrandName = @BrandName
          AND rbm.IsActive = 1
          AND r.IsActive = 1
        ORDER BY r.RegionName";
    public const string GetQueuesByBrand = @"
        SELECT q.QueueName
        FROM dbo.QueueMaster q
        INNER JOIN dbo.BrandQueueMapping bqm ON bqm.QueueId = q.QueueId
        INNER JOIN dbo.BrandMaster b ON b.BrandId = bqm.BrandId
        WHERE b.BrandName = @BrandName
          AND bqm.IsActive = 1
          AND q.IsActive = 1
        ORDER BY q.QueueName";

    public const string GetByOrderSeq = @"
        SELECT orderGUID, orderSeq, brand, repPO, AccountNumber, orderDate,
               repCustomer, repSalesPerson, jobNumber, repUserName, status,
               totalNet, OrderRecdDate, SalesOrderNumber, Priority, RepName,
               QueueName, ProductType, Region, JobName, CreatedDate,
               CompletionDate, PackageOwner
        FROM dbo.EdgeOrders
        WHERE orderSeq = @OrderSeq";

    public const string GetRouteToDepartmentQueues = @"
        SELECT q.QueueName
        FROM dbo.QueueMaster q
        INNER JOIN dbo.BrandQueueMapping bqm ON bqm.QueueId = q.QueueId
        INNER JOIN dbo.BrandMaster b ON b.BrandId = bqm.BrandId
        WHERE b.BrandName = @BrandName
          AND bqm.IsActive = 1
          AND q.IsActive = 1
        ORDER BY q.QueueName";
}
