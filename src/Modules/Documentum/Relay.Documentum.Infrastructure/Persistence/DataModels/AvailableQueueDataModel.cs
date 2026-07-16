using System.Data;
using Relay.Documentum.Contracts.Dtos;

namespace Relay.Documentum.Infrastructure.Persistence.DataModels;

internal sealed class AvailableQueueDataModel
{
    public int QueueId { get; init; }
    public string QueueName { get; init; } = default!;

    public static AvailableQueueDataModel FromRecord(IDataRecord record) => new()
    {
        QueueId   = record.GetInt32(record.GetOrdinal("QueueId")),
        QueueName = record.GetString(record.GetOrdinal("QueueName")),
    };

    public AvailableQueueDto ToDto() => new(QueueId, QueueName);
}
