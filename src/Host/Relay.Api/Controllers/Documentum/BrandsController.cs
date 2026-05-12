using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Api.Routes;
using Relay.Documentum.Application.Queries.GetAllBrands;
using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Api.Controllers.Documentum;

[ApiController]
[Authorize]
public sealed class DocumentumBrandsController : ControllerBase
{
    private readonly IQueryDispatcher _queries;

    public DocumentumBrandsController(IQueryDispatcher queries)
    {
        _queries = queries ?? throw new ArgumentNullException(nameof(queries));
    }

    [HttpGet(ApiRoutes.DocumentumBrands.GetAll)]
    [ProducesResponseType(typeof(List<BrandDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<GetAllBrandsQuery, IReadOnlyList<BrandDto>>(
            new GetAllBrandsQuery(), cancellationToken);

        return Ok(result.Value);
    }
}
