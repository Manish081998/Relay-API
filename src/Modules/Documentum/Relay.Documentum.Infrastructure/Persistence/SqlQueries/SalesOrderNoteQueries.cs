namespace Relay.Documentum.Infrastructure.Persistence.SqlQueries;

internal static class SalesOrderNoteQueries
{
    public const string Add          = "dbo.usp_AddSalesOrderNote";
    public const string GetByOrderSeq = "dbo.usp_GetSalesOrderNotes";
}
