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
        SELECT EO.orderGUID, EO.orderSeq, EO.brand, EO.repPO, EO.AccountNumber,
               EO.orderDate, EO.repCustomer, EO.repSalesPerson, EO.jobNumber,
               EO.repUserName, EO.status, EO.totalNet, EO.OrderRecdDate,
               EOD.SalesOrderNumber, EOD.Priority, EOD.RepName,
               EOD.QueueName, EOD.ProductType, EOD.Region, EOD.JobName,
               EOD.CreatedDate, EOD.CompletionDate,
               EO.repUserName AS PackageOwner
        FROM dbo.EdgeOrders EO
        LEFT JOIN dbo.EdgeOrderDetails EOD ON EO.orderSeq = EOD.orderSeq AND EOD.IsActive = 1
        WHERE EO.orderSeq = @OrderSeq";

    public const string GetRouteToDepartmentQueues = @"
        SELECT q.QueueId, q.QueueName
        FROM dbo.QueueMaster q
        INNER JOIN dbo.BrandQueueMapping bqm ON bqm.QueueId = q.QueueId
        INNER JOIN dbo.BrandMaster b ON b.BrandId = bqm.BrandId
        WHERE b.BrandName = @BrandName
          AND bqm.IsActive = 1
          AND q.IsActive = 1
        ORDER BY q.QueueName";
}
