using System.Data;
using Relay.Documentum.Contracts.Dtos;

namespace Relay.Documentum.Infrastructure.Persistence.DataModels;

internal sealed class QueueUserMappingDataModel
{
    public string? FullName { get; init; }
    public string? GlobalId { get; init; }
    public string? EmailId { get; init; }
    public int? BrandId { get; init; }
    public string? BrandName { get; init; }
    public string? QueueId { get; init; }
    public string? QueueName { get; init; }
    public string? ActionByFullName { get; init; }
    public int? RoleMasterId { get; init; }
    public string? RoleName { get; init; }

    public static QueueUserMappingDataModel FromRecord(IDataRecord record)
    {
        static string? NullableString(IDataRecord r, string col)
        {
            var ord = r.GetOrdinal(col);
            return r.IsDBNull(ord) ? null : r.GetString(ord);
        }

        static int? NullableInt(IDataRecord r, string col)
        {
            var ord = r.GetOrdinal(col);
            return r.IsDBNull(ord) ? null : r.GetInt32(ord);
        }

        return new()
        {
            FullName = NullableString(record, "FullName"),
            GlobalId = NullableString(record, "GlobalID"),
            EmailId = NullableString(record, "EmailID"),
            BrandId = NullableInt(record, "BrandId"),
            BrandName = NullableString(record, "BrandName"),
            QueueId = NullableString(record, "QueueId"),
            QueueName = NullableString(record, "QueueName"),
            ActionByFullName = NullableString(record, "ActionByFullName"),
            RoleMasterId = NullableInt(record, "RoleMasterId"),
            RoleName = NullableString(record, "RoleName"),
        };
    }

    public QueueUserMappingDto ToDto() =>
        new(FullName, GlobalId, EmailId, BrandId, BrandName, QueueId, QueueName, ActionByFullName, RoleMasterId, RoleName);
}
