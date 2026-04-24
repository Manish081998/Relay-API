using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Api.Routes;
using Relay.SharedKernel.Application;
using Relay.WebTool.Application.Queries.GetSelectionById;
using Relay.WebTool.Contracts.Dtos;

namespace Relay.Api.Controllers.WebTool;

[Authorize]
[ApiController]
public sealed class SelectionsController : ControllerBase
{
    private readonly IQueryDispatcher _queries;

    public SelectionsController(IQueryDispatcher queries)
    {
        _queries = queries ?? throw new ArgumentNullException(nameof(queries));
    }

    [HttpGet(ApiRoutes.Selections.GetById)]
    [ProducesResponseType(typeof(SelectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<GetSelectionByIdQuery, SelectionDto?>(new GetSelectionByIdQuery(id), cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : NotFound();
    }
}
