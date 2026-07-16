using Microsoft.AspNetCore.Http;

namespace Relay.Api.Requests.Documentum;

public sealed record CreateDocumentVersionRequest(
    int DocumentId,
    IFormFile File,
    string? Comment);
