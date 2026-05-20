namespace Relay.Documentum.Contracts.Dtos;

public sealed record BrandQueueMappingResultDto(
    IReadOnlyList<BrandDto> Brands,
    IReadOnlyList<AvailableQueueDto> AvailableQueues,
    IReadOnlyList<SelectedQueueDto> SelectedQueues);
