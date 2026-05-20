using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Api.Requests.Documentum;
using Relay.Api.Routes;
using Relay.Documentum.Application.Commands.CreateDocumentVersion;
using Relay.Documentum.Application.Commands.UploadSalesOrderDocument;
using Relay.Documentum.Application.Queries.GetDocumentsByOrderSeq;
using Relay.Documentum.Application.Queries.GetDocumentVersions;
using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Api.Controllers.Documentum;

[ApiController]
[Authorize]
public sealed class SalesOrderDocumentController : ControllerBase
{
    private readonly IQueryDispatcher _queries;
    private readonly ICommandDispatcher _commands;
    private readonly IWebHostEnvironment _env;

    public SalesOrderDocumentController(
        IQueryDispatcher queries,
        ICommandDispatcher commands,
        IWebHostEnvironment env)
    {
        _queries  = queries  ?? throw new ArgumentNullException(nameof(queries));
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        _env      = env      ?? throw new ArgumentNullException(nameof(env));
    }

    // ─── Upload new document ────────────────────────────────────────────────

    [HttpPost(ApiRoutes.Documentum.SalesOrderDocuments.Upload)]
    [ProducesResponseType(typeof(UploadDocumentResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(
        [FromForm] UploadSalesOrderDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest("File is required.");

        var safeFileName = SanitizeFileName(request.File.FileName);
        var mimeType     = request.File.ContentType;
        var contentType  = GetContentTypeCategory(safeFileName);

        // Build storage: FileStorage/assets/orders/{orderSeq}/{filename}/v1_{filename}
        var folderRelative = Path.Combine("assets", "orders", request.OrderSeq.ToString(), safeFileName);
        var folderAbsolute = Path.Combine(_env.ContentRootPath, "FileStorage", folderRelative);
        Directory.CreateDirectory(folderAbsolute);

        var versionFileName = $"v1_{safeFileName}";
        var filePath = Path.Combine(folderAbsolute, versionFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.File.CopyToAsync(stream, cancellationToken);
        }

        var documentPathRelative = Path.Combine(folderRelative, versionFileName).Replace('\\', '/');

        var command = new UploadSalesOrderDocumentCommand(
            request.OrderSeq, request.RepPO, request.BrandName, safeFileName,
            contentType, mimeType, request.File.Length, documentPathRelative,
            request.IsSupportedDocument, User.Identity?.Name ?? "system");

        var result = await _commands.SendAsync(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }

    // ─── Create new version (edit/annotate) ─────────────────────────────────

    [HttpPost(ApiRoutes.Documentum.SalesOrderDocuments.CreateVersion)]
    [ProducesResponseType(typeof(UploadDocumentResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVersion(
        [FromForm] CreateDocumentVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest("File is required.");

        // Get existing versions to determine storage folder and next version number
        var versionsResult = await _queries.SendAsync<GetDocumentVersionsQuery, IReadOnlyList<SalesOrderDocumentVersionDto>>(
            new GetDocumentVersionsQuery(request.DocumentId), cancellationToken);

        var latestVersion = versionsResult.IsSuccess && versionsResult.Value.Count > 0
            ? versionsResult.Value[0]   // ordered DESC
            : null;

        var nextVersion = latestVersion is not null ? latestVersion.VersionNumber + 1 : 1;

        // Determine storage folder from the latest version's path
        string folderAbsolute;
        string folderRelative;

        if (latestVersion is not null)
        {
            folderRelative = Path.GetDirectoryName(latestVersion.DocumentPath)?.Replace('\\', '/') ?? "";
            folderAbsolute = Path.Combine(_env.ContentRootPath, "FileStorage",
                folderRelative.Replace('/', Path.DirectorySeparatorChar));
        }
        else
        {
            var safeName = SanitizeFileName(request.File.FileName);
            folderRelative = $"assets/orders/unknown/{safeName}";
            folderAbsolute = Path.Combine(_env.ContentRootPath, "FileStorage",
                folderRelative.Replace('/', Path.DirectorySeparatorChar));
        }

        Directory.CreateDirectory(folderAbsolute);

        var safeFileName = SanitizeFileName(request.File.FileName);
        var versionFileName = $"v{nextVersion}_{safeFileName}";
        var filePath = Path.Combine(folderAbsolute, versionFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.File.CopyToAsync(stream, cancellationToken);
        }

        var documentPathRelative = $"{folderRelative}/{versionFileName}";
        var mimeType    = request.File.ContentType;
        var contentType = GetContentTypeCategory(safeFileName);

        var command = new CreateDocumentVersionCommand(
            request.DocumentId, documentPathRelative, contentType, mimeType,
            request.File.Length, User.Identity?.Name ?? "system", request.Comment);

        var result = await _commands.SendAsync(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }

    // ─── Get documents by orderSeq ──────────────────────────────────────────

    [HttpGet(ApiRoutes.Documentum.SalesOrderDocuments.GetByOrderSeq)]
    [ProducesResponseType(typeof(IReadOnlyList<SalesOrderDocumentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOrderSeq(
        [FromRoute] int orderSeq,
        [FromQuery] bool? isSupportedDocument = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<GetDocumentsByOrderSeqQuery, IReadOnlyList<SalesOrderDocumentDto>>(
            new GetDocumentsByOrderSeqQuery(orderSeq, isSupportedDocument), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }

    // ─── Get document versions ──────────────────────────────────────────────

    [HttpGet(ApiRoutes.Documentum.SalesOrderDocuments.GetVersions)]
    [ProducesResponseType(typeof(IReadOnlyList<SalesOrderDocumentVersionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVersions(
        [FromRoute] int documentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<GetDocumentVersionsQuery, IReadOnlyList<SalesOrderDocumentVersionDto>>(
            new GetDocumentVersionsQuery(documentId), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }

    // ─── Serve file for preview ─────────────────────────────────────────────

    [HttpGet(ApiRoutes.Documentum.SalesOrderDocuments.Preview)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Preview([FromQuery] string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return BadRequest("Path is required.");

        // Security: prevent directory traversal
        var sanitized = path.Replace("..", "").Replace('\\', '/');
        var fullPath = Path.Combine(_env.ContentRootPath, "FileStorage",
            sanitized.Replace('/', Path.DirectorySeparatorChar));

        if (!System.IO.File.Exists(fullPath))
            return NotFound("File not found.");

        var mimeType = GetMimeType(fullPath);
        var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(fileStream, mimeType);
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(c => !invalid.Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "document" : sanitized;
    }

    private static string GetContentTypeCategory(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".pdf"  => "PDF",
            ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" or ".tiff" or ".tif" or ".svg" => "Image",
            ".doc" or ".docx" => "Word",
            ".xls" or ".xlsx" => "Excel",
            ".txt"  => "Text",
            _       => "Other",
        };

    private static string GetMimeType(string filePath) =>
        Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".pdf"  => "application/pdf",
            ".png"  => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif"  => "image/gif",
            ".bmp"  => "image/bmp",
            ".tiff" or ".tif" => "image/tiff",
            ".svg"  => "image/svg+xml",
            ".doc"  => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls"  => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt"  => "text/plain",
            _       => "application/octet-stream",
        };
}
