namespace Relay.Api.Requests.Documentum;

public sealed record CompleteWorkflowRequest(int DestinationQueueId, string? DisplayName = null);
