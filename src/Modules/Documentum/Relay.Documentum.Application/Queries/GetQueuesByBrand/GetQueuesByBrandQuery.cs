using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetQueuesByBrand;

public sealed record GetQueuesByBrandQuery(string BrandName) : IQuery<IReadOnlyList<string>>;
