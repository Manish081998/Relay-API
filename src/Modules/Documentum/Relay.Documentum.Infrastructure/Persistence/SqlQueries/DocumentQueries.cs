namespace Relay.Documentum.Infrastructure.Persistence.SqlQueries;

internal static class DocumentQueries
{
    public const string Update = "documentum.sp_UpdateDocument";
    public const string GetById = "documentum.sp_GetDocumentById";
    public const string GetByName = "documentum.sp_SearchDocumentsByName";
}
