namespace Relay.Documentum.Contracts.Dtos;

public sealed record BrandAndQueuesAndMappingDto(
    IReadOnlyList<BrandDto> Brands,
    IReadOnlyList<BrandQueueMappingDto> BrandQueueMappings,
    IReadOnlyList<QueueUserMappingDto> UserQueueMappings,
    IReadOnlyList<RoleDto> Roles);
