using System.Data;
using Relay.Documentum.Contracts.Dtos;

namespace Relay.Documentum.Infrastructure.Persistence.DataModels;

internal sealed class BrandQueueMappingDataModel
{
    public int BrandId { get; init; }
    public string BrandName { get; init; } = default!;
    public int QueueId { get; init; }
    public string QueueName { get; init; } = default!;

    public static BrandQueueMappingDataModel FromRecord(IDataRecord record) => new()
    {
        BrandId   = record.GetInt32(record.GetOrdinal("BrandId")),
        BrandName = record.GetString(record.GetOrdinal("BrandName")),
        QueueId   = record.GetInt32(record.GetOrdinal("QueueId")),
        QueueName = record.GetString(record.GetOrdinal("QueueName")),
    };

    public BrandQueueMappingDto ToDto() => new(BrandId, BrandName, QueueId, QueueName);
}
