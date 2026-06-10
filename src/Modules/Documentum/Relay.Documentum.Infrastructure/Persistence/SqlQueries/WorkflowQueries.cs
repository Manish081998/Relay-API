namespace Relay.Documentum.Infrastructure.Persistence.SqlQueries;

internal static class WorkflowQueries
{
    public const string ProcessAction = "dbo.usp_ProcessWorkflowAction";
    public const string GetState      = "dbo.usp_GetWorkflowState";
    public const string GetHistory    = "dbo.usp_GetWorkflowHistory";
}
