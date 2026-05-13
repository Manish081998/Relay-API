using System.Data;
using Relay.Documentum.Contracts.Dtos;

namespace Relay.Documentum.Infrastructure.Persistence.DataModels;

internal sealed class RoleDataModel
{
    public int RoleMasterId { get; init; }
    public string RoleName { get; init; } = default!;
    public int PrivilegeLevel { get; init; }

    public static RoleDataModel FromRecord(IDataRecord record) => new()
    {
        RoleMasterId   = record.GetInt32(record.GetOrdinal("RoleMasterID")),
        RoleName       = record.GetString(record.GetOrdinal("RoleName")),
        PrivilegeLevel = record.GetInt32(record.GetOrdinal("PrivilegeLevel")),
    };

    public RoleDto ToDto() => new(RoleMasterId, RoleName, PrivilegeLevel);
}
