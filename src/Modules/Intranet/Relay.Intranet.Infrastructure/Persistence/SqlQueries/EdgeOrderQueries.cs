namespace Relay.Intranet.Infrastructure.Persistence.SqlQueries;

internal static class EdgeOrderQueries
{
    public const string Search        = "dbo.usp_Intranet_SearchEdgeOrders";
    public const string GetByOrderGuid = "dbo.GetOrderByOrderGUID";
}
