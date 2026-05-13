using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetAllBrands;

public sealed record GetAllBrandsQuery() : IQuery<IReadOnlyList<BrandDto>>;
