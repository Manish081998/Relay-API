using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetBrandAndQueuesAndMapping;

public sealed record GetBrandAndQueuesAndMappingQuery() : IQuery<BrandAndQueuesAndMappingDto>;
