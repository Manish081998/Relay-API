using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relay.Api.Routes;
using Relay.Intranet.Application.Queries.GetCountries;
using Relay.Intranet.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Api.Controllers.Intranet;

[Authorize]
[ApiController]
public sealed class CountriesController : ControllerBase
{
    private readonly IQueryDispatcher _queries;

    public CountriesController(IQueryDispatcher queries)
    {
        _queries = queries ?? throw new ArgumentNullException(nameof(queries));
    }

    [HttpGet(ApiRoutes.Intranet.Countries.GetAll)]
    [ProducesResponseType(typeof(IReadOnlyList<CountryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string brand, CancellationToken cancellationToken = default)
    {
        var result = await _queries.SendAsync<GetCountriesQuery, IReadOnlyList<CountryDto>>(
            new GetCountriesQuery(brand), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error.Description);
    }
}
