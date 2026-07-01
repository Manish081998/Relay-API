using Microsoft.AspNetCore.Http;

namespace Relay.Api.Requests.Documentum;

public sealed record UploadSalesOrderDocumentRequest(
    int OrderSeq,
    string? RepPO,
    string? BrandName,
    string? OrderDate,
    bool IsSupportedDocument,
    IFormFile File);
