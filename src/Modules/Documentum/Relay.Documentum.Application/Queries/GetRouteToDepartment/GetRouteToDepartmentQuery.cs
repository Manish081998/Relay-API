using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetRouteToDepartment;

public sealed record GetRouteToDepartmentQuery(string BrandName) : IQuery<IReadOnlyList<string>>;
