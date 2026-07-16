using System;
using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Application.Tests.Builders;

internal static class DocumentBuilder
{
    public static Document Build(
        Guid? id = null,
        string title = "Test Document",
        string storagePath = "/docs/test.pdf",
        Guid? ownerId = null,
        int statusId = 1,
        long sizeInBytes = 1024,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? publishedAt = null) =>
        Document.Reconstitute(
            id ?? Guid.NewGuid(),
            title,
            storagePath,
            ownerId ?? Guid.NewGuid(),
            statusId,
            sizeInBytes,
            createdAt ?? DateTimeOffset.UtcNow,
            publishedAt);
}
