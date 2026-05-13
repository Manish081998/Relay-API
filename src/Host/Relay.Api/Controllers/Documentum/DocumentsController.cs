using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Api.Requests.Documentum;
using Relay.Api.Routes;
using Relay.Documentum.Application.Commands.UpdateDocumentById;
using Relay.Documentum.Application.Queries.GetDocumentById;
using Relay.Documentum.Application.Queries.GetDocumentByName;
using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Api.Controllers.Documentum;

[ApiController]
[Authorize]
public sealed class DocumentsController : ControllerBase
{
    private readonly IQueryDispatcher _queries;
    private readonly ICommandDispatcher _commands;

    public DocumentsController(IQueryDispatcher queries, ICommandDispatcher commands)
    {
        _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
    }

    [HttpGet(ApiRoutes.Documentum.Documents.GetById)]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<GetDocumentByIdQuery, DocumentDto?>(new GetDocumentByIdQuery(id), cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : NotFound();
    }

    [HttpGet(ApiRoutes.Documentum.Documents.Search)]
    [ProducesResponseType(typeof(IReadOnlyList<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByName([FromQuery] string name, CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<GetDocumentByNameQuery, IReadOnlyList<DocumentDto>>(new GetDocumentByNameQuery(name), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }

    [HttpPut(ApiRoutes.Documentum.Documents.UpdateById)]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateById(Guid id, [FromBody] UpdateDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _commands.SendAsync(new UpdateDocumentByIdCommand(id, request.Title, request.StoragePath, request.SizeInBytes),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Code == "Document.NotFound"
                ? NotFound(result.Error.Description)
                : BadRequest(result.Error.Description);
        }

        return Ok(result.Value);
    }
}
