using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetProductTypes;

public sealed record GetProductTypesQuery : IQuery<IReadOnlyList<string>>;
