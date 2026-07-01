using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Api.Requests.Documentum;
using Relay.Api.Routes;
using Relay.Api.Services;
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
    private readonly IFileStorageService _storage;
    private readonly IDocumentPathBuilder _pathBuilder;
    private readonly ILogger<SalesOrderDocumentController> _logger;

    private string CurrentUserName => User.Identity?.Name ?? "system";

    public SalesOrderDocumentController(
        IQueryDispatcher queries,
        ICommandDispatcher commands,
        IFileStorageService storage,
        IDocumentPathBuilder pathBuilder,
        ILogger<SalesOrderDocumentController> logger)
    {
        _queries     = queries     ?? throw new ArgumentNullException(nameof(queries));
        _commands    = commands    ?? throw new ArgumentNullException(nameof(commands));
        _storage     = storage     ?? throw new ArgumentNullException(nameof(storage));
        _pathBuilder = pathBuilder ?? throw new ArgumentNullException(nameof(pathBuilder));
        _logger      = logger     ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost(ApiRoutes.Documentum.SalesOrderDocuments.Upload)]
    [ProducesResponseType(typeof(UploadDocumentResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(
        [FromForm] UploadSalesOrderDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest("File is required.");

        if (request.OrderSeq <= 0)
            return BadRequest("OrderSeq must be greater than zero.");

        var originalFileName = DocumentFileHelper.SanitizeFileName(request.File.FileName);
        var mimeType         = request.File.ContentType;
        var contentType      = DocumentFileHelper.GetContentTypeCategory(originalFileName);
        var documentName     = DocumentFileHelper.BuildDocumentDisplayName(originalFileName, request.RepPO, request.IsSupportedDocument);

        var orderDate = DateTime.TryParse(request.OrderDate, out var parsedDate)
            ? parsedDate
            : DateTime.Now;

        var brand = !string.IsNullOrWhiteSpace(request.BrandName) ? request.BrandName : "Unknown";

        var folderRelative  = _pathBuilder.BuildRelativeFolder(brand, orderDate, request.OrderSeq);
        var folderAbsolute  = _pathBuilder.BuildAbsoluteFolder(brand, orderDate, request.OrderSeq);
        var versionFileName = $"v1_{documentName}";
        var filePath        = Path.Combine(folderAbsolute, versionFileName);

        try
        {
            await using var stream = request.File.OpenReadStream();
            await _storage.SaveFileAsync(filePath, stream, cancellationToken);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to save uploaded file for OrderSeq {OrderSeq}", request.OrderSeq);
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save file to storage.");
        }

        var documentPathRelative = $"{folderRelative}/{versionFileName}".Replace('\\', '/');

        var command = new UploadSalesOrderDocumentCommand(
            request.OrderSeq, request.RepPO, request.BrandName, documentName,
            contentType, mimeType, request.File.Length, documentPathRelative,
            request.IsSupportedDocument, CurrentUserName);

        var result = await _commands.SendAsync(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }

    [HttpPost(ApiRoutes.Documentum.SalesOrderDocuments.CreateVersion)]
    [ProducesResponseType(typeof(UploadDocumentResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVersion(
        [FromForm] CreateDocumentVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest("File is required.");

        if (request.DocumentId <= 0)
            return BadRequest("DocumentId must be greater than zero.");

        var versionsResult = await _queries.SendAsync<GetDocumentVersionsQuery, IReadOnlyList<SalesOrderDocumentVersionDto>>(
            new GetDocumentVersionsQuery(request.DocumentId), cancellationToken);

        var latestVersion = versionsResult.IsSuccess && versionsResult.Value.Count > 0
            ? versionsResult.Value[0]
            : null;

        var nextVersion = latestVersion is not null ? latestVersion.VersionNumber + 1 : 1;

        string folderRelative;
        if (latestVersion is not null)
        {
            folderRelative = Path.GetDirectoryName(latestVersion.DocumentPath)?.Replace('\\', '/') ?? "";
        }
        else
        {
            var safeName = DocumentFileHelper.SanitizeFileName(request.File.FileName);
            folderRelative = $"unknown/{safeName}";
        }

        var folderAbsolute = Path.Combine(_storage.BasePath,
            folderRelative.Replace('/', Path.DirectorySeparatorChar));

        var latestFileName  = Path.GetFileName(latestVersion?.DocumentPath ?? "");
        var documentName    = DocumentFileHelper.ExtractOriginalName(latestFileName);
        var versionFileName = $"v{nextVersion}_{documentName}";
        var filePath        = Path.Combine(folderAbsolute, versionFileName);

        try
        {
            await using var stream = request.File.OpenReadStream();
            await _storage.SaveFileAsync(filePath, stream, cancellationToken);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to save version file for DocumentId {DocumentId}", request.DocumentId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save file to storage.");
        }

        var documentPathRelative = $"{folderRelative}/{versionFileName}";
        var mimeType    = request.File.ContentType;
        var contentType = DocumentFileHelper.GetContentTypeCategory(documentName);

        var command = new CreateDocumentVersionCommand(
            request.DocumentId, documentPathRelative, contentType, mimeType,
            request.File.Length, CurrentUserName, request.Comment);

        var result = await _commands.SendAsync(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }

    [HttpGet(ApiRoutes.Documentum.SalesOrderDocuments.GetByOrderSeq)]
    [ProducesResponseType(typeof(IReadOnlyList<SalesOrderDocumentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOrderSeq(
        [FromRoute] int orderSeq,
        [FromQuery] bool? isSupportedDocument = null,
        CancellationToken cancellationToken = default)
    {
        if (orderSeq <= 0)
            return BadRequest("OrderSeq must be greater than zero.");

        var result = await _queries.SendAsync<GetDocumentsByOrderSeqQuery, IReadOnlyList<SalesOrderDocumentDto>>(
            new GetDocumentsByOrderSeqQuery(orderSeq, isSupportedDocument), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }

    [HttpGet(ApiRoutes.Documentum.SalesOrderDocuments.GetVersions)]
    [ProducesResponseType(typeof(IReadOnlyList<SalesOrderDocumentVersionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVersions(
        [FromRoute] int documentId,
        CancellationToken cancellationToken = default)
    {
        if (documentId <= 0)
            return BadRequest("DocumentId must be greater than zero.");

        var result = await _queries.SendAsync<GetDocumentVersionsQuery, IReadOnlyList<SalesOrderDocumentVersionDto>>(
            new GetDocumentVersionsQuery(documentId), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }

    [HttpGet(ApiRoutes.Documentum.SalesOrderDocuments.Preview)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Preview([FromQuery] string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return BadRequest("Path is required.");

        var sanitized = path.Replace("..", "").Replace('\\', '/');
        var fullPath = Path.Combine(_storage.BasePath,
            sanitized.Replace('/', Path.DirectorySeparatorChar));

        if (!_storage.FileExists(fullPath))
            return NotFound("File not found.");

        var mimeType   = DocumentFileHelper.GetMimeType(fullPath);
        var fileStream = _storage.OpenRead(fullPath);
        return File(fileStream, mimeType);
    }
}
