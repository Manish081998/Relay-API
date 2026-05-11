using System.Data;
using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Infrastructure.Persistence.DataModels;

internal sealed class UserDataModel
{
    public Guid UserId { get; init; }
    public string GlobalId { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string? BrandName { get; init; }
    public Guid BrandId { get; init; }
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
        UserId      = record.GetGuid(record.GetOrdinal("UserId")),
        GlobalId    = record.GetString(record.GetOrdinal("GlobalId")),
        Password    = record.GetString(record.GetOrdinal("Password")),
        BrandId = record.GetGuid(record.GetOrdinal("BrandId")),
        BrandName = record.GetString(record.GetOrdinal("BrandName")),
        FirstName   = record.GetString(record.GetOrdinal("FirstName")),
        LastName    = record.GetString(record.GetOrdinal("LastName")),
        EmailId    = record.GetString(record.GetOrdinal("EmailId")),
        IsActive    = record.GetBoolean(record.GetOrdinal("IsActive")),
        CreatedBy   = record.GetString(record.GetOrdinal("CreatedBy")),
        ModifiedBy   = record.GetString(record.GetOrdinal("ModifiedBy")),
        CreatedDate = record.GetDateTime(record.GetOrdinal("CreatedDate")),
        ModifiedDate = record.GetDateTime(record.GetOrdinal("ModifiedDate"))
    };

    public User ToDomain() => new(
        UserId:      UserId,
        GlobalId:    GlobalId,
        Password:    Password,
        FirstName:   FirstName,
        LastName:    LastName,
        EmailId:     EmailId,
        BrandName:   BrandName,
        BrandId : BrandId,
        IsActive:    IsActive,
        CreatedBy:   CreatedBy,
        CreatedDate: CreatedDate,
        ModifiedBy:  ModifiedBy,
        ModifiedDate: ModifiedDate);
}
