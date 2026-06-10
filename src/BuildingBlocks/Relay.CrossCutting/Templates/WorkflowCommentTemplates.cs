namespace Relay.CrossCutting.Templates;

public sealed class WorkflowCommentTemplates
{
    public string Acquire  { get; set; } = "{UserName} acquired this task from {SourceQueueName}";
    public string Unassign { get; set; } = "{UserName} unassigned task back to {CurrentQueueName}";
    public string Complete { get; set; } = "{UserName} routed task to {DestinationQueueName}";
}
