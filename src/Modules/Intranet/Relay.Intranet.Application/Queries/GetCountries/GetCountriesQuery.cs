using Relay.Intranet.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Queries.GetCountries;

public sealed record GetCountriesQuery(string Brand) : IQuery<IReadOnlyList<CountryDto>>;
