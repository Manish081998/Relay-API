using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetProductTypes;

public sealed record GetProductTypesQuery(string BrandName) : IQuery<IReadOnlyList<ProductTypeDto>>;
