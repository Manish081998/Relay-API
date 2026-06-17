namespace Relay.Api.Requests.Documentum;

public sealed record BulkAcquireRequest(int[] OrderSeqs, string? DisplayName = null);
