using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetRegionsByBrand;

public sealed record GetRegionsByBrandQuery(string BrandName) : IQuery<IReadOnlyList<RegionDto>>;
