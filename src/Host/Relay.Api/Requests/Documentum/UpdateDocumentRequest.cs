namespace Relay.Api.Requests.Documentum;

public sealed record UpdateDocumentRequest(string Title, string StoragePath, long SizeInBytes);
