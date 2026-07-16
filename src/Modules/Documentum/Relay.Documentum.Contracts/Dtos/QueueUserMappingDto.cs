namespace Relay.Documentum.Contracts.Dtos;

public sealed record QueueUserMappingDto(
    string? FullName,
    string? GlobalId,
    string? EmailId,
    int? BrandId,
    string? BrandName,
    string? QueueId,
    string? QueueName,
    string? ActionByFullName,
    int? RoleMasterId,
    string? RoleName);
