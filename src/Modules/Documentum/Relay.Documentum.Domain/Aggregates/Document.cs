using Relay.Documentum.Domain.Enumerations;
using Relay.Documentum.Domain.Exceptions;
using Relay.SharedKernel.Common;
using Relay.SharedKernel.Domain;

namespace Relay.Documentum.Domain.Aggregates;

public sealed class Document : AggregateRoot<Guid>
{
    public string Title { get; private set; } = null!;
    public string StoragePath { get; private set; } = null!;
    public Guid OwnerId { get; private set; }
    public DocumentStatus Status { get; private set; } = null!;
    public long SizeInBytes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }

    private Document() { }

    private Document(Guid id, string title, string storagePath, Guid ownerId,
                     DocumentStatus status, long sizeInBytes, DateTimeOffset createdAt, DateTimeOffset? publishedAt)
        : base(id)
    {
        Title = title;
        StoragePath = storagePath;
        OwnerId = ownerId;
        Status = status;
        SizeInBytes = sizeInBytes;
        CreatedAt = createdAt;
        PublishedAt = publishedAt;
    }

    internal static Document Reconstitute(
        Guid id, string title, string storagePath, Guid ownerId,
        int statusId, long sizeInBytes, DateTimeOffset createdAt, DateTimeOffset? publishedAt) =>
        new Document(id, title, storagePath, ownerId,
                     DocumentStatus.FromId(statusId), sizeInBytes, createdAt, publishedAt);

    public void UpdateDetails(string title, string storagePath, long sizeInBytes)
    {
        Guard.NotNullOrWhiteSpace(title);
        Guard.NotNullOrWhiteSpace(storagePath);
        if (sizeInBytes < 0)
        {
            throw new DocumentumDomainException("SizeInBytes cannot be negative.");
        }
        Title = title;
        StoragePath = storagePath;
        SizeInBytes = sizeInBytes;
    }
}
