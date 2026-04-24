namespace Relay.Intranet.Contracts.Dtos;

/// <summary>
/// Read model exposed to other modules and to the outside world.
/// </summary>
public sealed record UserDto(
    Guid Id,
    string DisplayName,
    string Email,
    bool IsActive,
    DateTimeOffset CreatedAt);
