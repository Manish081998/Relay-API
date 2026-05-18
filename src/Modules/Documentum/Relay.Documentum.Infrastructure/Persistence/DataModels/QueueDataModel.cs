using System.Data;
using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Infrastructure.Persistence.DataModels;

internal sealed class QueueDataModel
{
    public int QueueId { get; init; }
    public string QueueName { get; init; } = default!;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public string CreatedBy { get; init; } = default!;
    public DateTime CreatedDate { get; init; }
    public string? ModifiedBy { get; init; }
    public DateTime? ModifiedDate { get; init; }

    public static QueueDataModel FromRecord(IDataRecord record) => new()
    {
        QueueId     = record.GetInt32(record.GetOrdinal("QueueId")),
        QueueName   = record.GetString(record.GetOrdinal("QueueName")),
        Description = record.IsDBNull(record.GetOrdinal("Description")) ? null : record.GetString(record.GetOrdinal("Description")),
        IsActive    = record.GetBoolean(record.GetOrdinal("IsActive")),
        CreatedBy   = record.GetString(record.GetOrdinal("CreatedBy")),
        CreatedDate = record.GetDateTime(record.GetOrdinal("CreatedDate")),
        ModifiedBy  = record.IsDBNull(record.GetOrdinal("ModifiedBy")) ? null : record.GetString(record.GetOrdinal("ModifiedBy")),
        ModifiedDate = record.IsDBNull(record.GetOrdinal("ModifiedDate")) ? null : record.GetDateTime(record.GetOrdinal("ModifiedDate")),
    };

    public Queue ToDomain() => new(
        QueueId:     QueueId,
        QueueName:   QueueName,
        Description: Description,
        IsActive:    IsActive,
        CreatedBy:   CreatedBy,
        CreatedDate: CreatedDate,
        ModifiedBy:  ModifiedBy,
        ModifiedDate: ModifiedDate);
}
