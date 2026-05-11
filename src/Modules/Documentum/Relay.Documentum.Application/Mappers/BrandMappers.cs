using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Application.Mappers;

internal static class BrandMappers
{
    public static BrandDto ToDto(this Brand brand) =>
        new(brand.BrandId, brand.BrandName);
}
