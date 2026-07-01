using Relay.Intranet.Contracts.Dtos;
using Relay.Intranet.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Queries.GetCountries;

internal sealed class GetCountriesQueryHandler : IQueryHandler<GetCountriesQuery, IReadOnlyList<CountryDto>>
{
    private readonly ICountryRepository _countries;

    public GetCountriesQueryHandler(ICountryRepository countries)
    {
        _countries = countries ?? throw new ArgumentNullException(nameof(countries));
    }

    public async Task<Result<IReadOnlyList<CountryDto>>> HandleAsync(
        GetCountriesQuery query, CancellationToken cancellationToken = default)
    {
        var items = await _countries.GetAllAsync(query.Brand, cancellationToken);

        return Result.Success<IReadOnlyList<CountryDto>>(
            items.Select(c => new CountryDto(c.code, c.name)).ToList());
    }
}
