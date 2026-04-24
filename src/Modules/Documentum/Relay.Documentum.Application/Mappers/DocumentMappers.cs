using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Application.Mappers;

internal static class DocumentMappers
{
    public static DocumentDto ToDto(this Document document) =>
        new DocumentDto(
            document.Id,
            document.Title,
            document.Status.ToString(),
            document.OwnerId,
            document.SizeInBytes,
            document.CreatedAt,
            document.PublishedAt);
}