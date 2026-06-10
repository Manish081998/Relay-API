using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetRouteToDepartment;

public sealed record GetRouteToDepartmentQuery(string BrandName) : IQuery<IReadOnlyList<RouteToDepartmentDto>>;
