namespace Relay.Intranet.Infrastructure.Persistence.SqlQueries;

internal static class UserQueries
{
    public const string Update = "documentum.sp_UpdateDocument";
    public const string GetById = "documentum.sp_GetDocumentById";
    public const string GetByName = "documentum.sp_SearchDocumentsByName";
}
