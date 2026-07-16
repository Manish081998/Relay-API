namespace Relay.Documentum.Infrastructure.Persistence.SqlQueries;

internal static class SalesOrderDocumentQueries
{
    public const string Upload         = "dbo.usp_UploadSalesOrderDocument";
    public const string CreateVersion  = "dbo.usp_CreateSalesOrderDocVersion";
    public const string GetByOrderSeq  = "dbo.usp_GetSalesOrderDocuments";
    public const string GetVersions    = "dbo.usp_GetSalesOrderDocVersions";
}
