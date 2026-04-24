using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Domain.Repositories;

public interface IAnnotationRepository
{
    Task<Annotation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
