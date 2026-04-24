using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Api.Routes;
using Relay.Documentum.Application.Queries.GetAnnotationDetailsById;
using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Api.Controllers.Documentum;

[ApiController]
[Authorize]
public sealed class AnnotationsController : ControllerBase
{
    private readonly IQueryDispatcher _queries;

    public AnnotationsController(IQueryDispatcher queries)
    {
        _queries = queries ?? throw new ArgumentNullException(nameof(queries));
    }

    [HttpGet(ApiRoutes.Annotations.GetById)]
    [ProducesResponseType(typeof(AnnotationDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAnnotationDetailsById(int id, CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<GetAnnotationDetailsByIdQuery, AnnotationDetailsDto>(new GetAnnotationDetailsByIdQuery(id), cancellationToken);
        return result.IsSuccess && result.Value is not null
            ? Ok(result.Value)
            : NotFound();
    }
}
