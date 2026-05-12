using System.Data;
using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Infrastructure.Persistence.DataModels;

internal sealed class UserDataModel
{
    public int UserId { get; init; }
    public string GlobalId { get; init; } = default!;
    public string? BrandName { get; init; }
    public int BrandId { get; init; }
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string EmailId { get; init; } = default!;
    public bool IsActive { get; init; }
    public string CreatedBy { get; init; } = default!;
    public string ModifiedBy { get; init; } = default!;
    public DateTime CreatedDate { get; init; }
    public DateTime ModifiedDate { get; init; }

    public static UserDataModel FromRecord(IDataRecord record) => new()
    {
        UserId       = record.GetInt32(record.GetOrdinal("UserId")),
        GlobalId     = record.GetString(record.GetOrdinal("GlobalId")),
        BrandId      = record.GetInt32(record.GetOrdinal("BrandId")),
        BrandName    = record.IsDBNull(record.GetOrdinal("BrandName")) ? null : record.GetString(record.GetOrdinal("BrandName")),
        FirstName    = record.GetString(record.GetOrdinal("FirstName")),
        LastName     = record.GetString(record.GetOrdinal("LastName")),
        EmailId      = record.GetString(record.GetOrdinal("EmailId")),
        IsActive     = record.GetBoolean(record.GetOrdinal("IsActive")),
        CreatedBy    = record.GetString(record.GetOrdinal("CreatedBy")),
        ModifiedBy   = record.IsDBNull(record.GetOrdinal("ModifiedBy")) ? string.Empty : record.GetString(record.GetOrdinal("ModifiedBy")),
        CreatedDate  = record.GetDateTime(record.GetOrdinal("CreatedDate")),
        ModifiedDate = record.IsDBNull(record.GetOrdinal("ModifiedDate")) ? default : record.GetDateTime(record.GetOrdinal("ModifiedDate")),
    };

    public User ToDomain() => new(
        UserId:       UserId,
        GlobalId:     GlobalId,
        FirstName:    FirstName,
        LastName:     LastName,
        EmailId:      EmailId,
        BrandId:      BrandId,
        BrandName:    BrandName,
        IsActive:     IsActive,
        CreatedBy:    CreatedBy,
        CreatedDate:  CreatedDate,
        ModifiedBy:   ModifiedBy,
        ModifiedDate: ModifiedDate);
}
