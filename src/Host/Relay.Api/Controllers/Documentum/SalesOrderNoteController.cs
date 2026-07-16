using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Api.Requests.Documentum;
using Relay.Api.Routes;
using Relay.Documentum.Application.Commands.AddSalesOrderNote;
using Relay.Documentum.Application.Queries.GetSalesOrderNotes;
using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Api.Controllers.Documentum;

[ApiController]
[Authorize]
public sealed class SalesOrderNoteController : ControllerBase
{
    private readonly IQueryDispatcher _queries;
    private readonly ICommandDispatcher _commands;

    public SalesOrderNoteController(
        IQueryDispatcher queries,
        ICommandDispatcher commands)
    {
        _queries  = queries  ?? throw new ArgumentNullException(nameof(queries));
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
    }

    // ─── Get notes by orderSeq ─────────────────────────────────────────────

    [HttpGet(ApiRoutes.Documentum.SalesOrderNotes.GetByOrderSeq)]
    [ProducesResponseType(typeof(IReadOnlyList<SalesOrderNoteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOrderSeq(
        [FromRoute] int orderSeq,
        CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<GetSalesOrderNotesQuery, IReadOnlyList<SalesOrderNoteDto>>(
            new GetSalesOrderNotesQuery(orderSeq), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }

    // ─── Add a new note ────────────────────────────────────────────────────

    [HttpPost(ApiRoutes.Documentum.SalesOrderNotes.Add)]
    [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add(
        [FromBody] AddSalesOrderNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NotesDescription))
            return BadRequest("Note description is required.");

        var command = new AddSalesOrderNoteCommand(
            request.OrderSeq,
            request.NotesDescription,
            User.Identity?.Name ?? "system");

        var result = await _commands.SendAsync(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error.Description);
    }
}
