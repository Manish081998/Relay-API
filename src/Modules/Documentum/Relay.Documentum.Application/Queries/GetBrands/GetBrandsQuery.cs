using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetBrands;

public sealed record GetBrandsQuery : IQuery<IReadOnlyList<string>>;
