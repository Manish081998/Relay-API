using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Application.Mappers;

internal static class QueueMappers
{
    public static QueueDto ToDto(this Queue queue) =>
        new QueueDto(
            QueueId:     queue.QueueId,
            QueueName:   queue.QueueName,
            Description: queue.Description,
            IsActive:    queue.IsActive,
            CreatedBy:   queue.CreatedBy,
            CreatedDate: queue.CreatedDate);
}
