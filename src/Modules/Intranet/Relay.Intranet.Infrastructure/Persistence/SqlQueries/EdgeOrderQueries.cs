namespace Relay.Intranet.Infrastructure.Persistence.SqlQueries;

internal static class EdgeOrderQueries
{
    public const string Search           = "dbo.usp_Intranet_SearchEdgeOrders";
    public const string GetByOrderGuid   = "dbo.GetOrderByOrderGUID";
    public const string TrackOrderChanges = "dbo.TrackOrderChanges";
    public const string GetEDISubmitStatus = "dbo.GetEDISubmitStatus";
    public const string TrackUserPO = "dbo.TrackUserPO";
    public const string ValidateState  = "SELECT COUNT(1) FROM States WHERE State_Code = @State AND Country = @Country";
    public const string GetEdiStatus   = "dbo.GetEDIStatus";
    public const string GetXref = "SELECT modelname, Macpacfield FROM xref WHERE (typenumeric = 1 OR typefractional = 1)";
    public const string GetMachPackFields = "SELECT FieldName FROM MacPacFieldTypes WHERE TypeFractional = 1";
}
