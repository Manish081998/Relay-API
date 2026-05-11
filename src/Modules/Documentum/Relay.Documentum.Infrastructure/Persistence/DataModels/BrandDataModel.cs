using System.Data;
using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Infrastructure.Persistence.DataModels;

internal sealed class BrandDataModel
{
    public Guid BrandId { get; init; }
    public string BrandName { get; init; } = default!;

    public static BrandDataModel FromRecord(IDataRecord record) => new()
    {
        BrandId   = record.GetGuid(record.GetOrdinal("BrandId")),
        BrandName = record.GetString(record.GetOrdinal("BrandName")),
    };

    public Brand ToDomain() => new(BrandId, BrandName);
}
