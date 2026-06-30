using System.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Relay.Infrastructure.Core.Data;

namespace Relay.Api.Services.Auth;

/// <summary>
/// Stored procedure names must match what exists in the OrderManagement database.
/// Update these constants if your SP names differ.
/// </summary>
internal static class UserAuthQueries
{
    public const string GetAuthStatus = "USP_UserAuthorizedStatus";
    public const string AddModifyUser = "USP_AddModifyUser";
    public const string DeleteUser    = "STP_DeleteUser";

    public const string GetUserBrandInfo = @"
        SELECT UM.BrandID,
               BM.BrandName,
               STRING_AGG(QM.QueueName, ',') AS AssociatedQueueNames
        FROM dbo.UserMaster UM
        INNER JOIN dbo.BrandMaster BM ON UM.BrandID = BM.BrandId
        LEFT JOIN dbo.QueueUserMapping QUM ON UM.UserId = QUM.UserId
        LEFT JOIN dbo.QueueMaster QM ON QUM.QueueID = QM.QueueId
        WHERE UM.GlobalID = @GlobalID
        GROUP BY UM.BrandID, BM.BrandName";
}

internal sealed class UserAuthRepository : IUserAuthRepository
{
    private const string Module = "Intranet";
    private readonly IDbExecutor _db;

    public UserAuthRepository(IDbExecutor db) => _db = db;

    public Task<UserAuthStatus?> GetAuthStatusAsync(string globalId, CancellationToken ct = default) =>
        _db.QuerySingleOrDefaultAsync(
            Module,
            UserAuthQueries.GetAuthStatus,
            r => new UserAuthStatus(
                Status: r.IsDBNull(r.GetOrdinal("Status")) ? string.Empty : r.GetString(r.GetOrdinal("Status")),
                Message: r.IsDBNull(r.GetOrdinal("Message")) ? string.Empty : r.GetString(r.GetOrdinal("Message")),
                UserType: r.IsDBNull(r.GetOrdinal("UserRoles")) ? "User" : r.GetString(r.GetOrdinal("UserRoles"))
            ),
            new { GlobalID = globalId },
            CommandType.StoredProcedure,
            ct);

    public Task<UserRecord?> UpsertUserAsync(AdUserDetails details, CancellationToken ct = default) =>
        _db.QuerySingleOrDefaultAsync(
            Module,
            UserAuthQueries.AddModifyUser,
            r => new UserRecord(
                UserId: r.GetInt32(r.GetOrdinal("UserId")),
                GlobalId: r.GetString(r.GetOrdinal("GlobalId")),
                FirstName: r.IsDBNull(r.GetOrdinal("FirstName")) ? null : r.GetString(r.GetOrdinal("FirstName")),
                LastName: r.IsDBNull(r.GetOrdinal("LastName")) ? null : r.GetString(r.GetOrdinal("LastName")),
                EmailId: r.IsDBNull(r.GetOrdinal("EmailId")) ? null : r.GetString(r.GetOrdinal("EmailId"))
            ),
            new
            {
                GlobalID = details.GlobalId,
                FirstName = details.FirstName,
                LastName = details.LastName,
                EmailId = details.EmailId,
                BrandID = details.BrandId,
                QueueId = details.QueueId,
                RoleId = details.RoleId
            },
            CommandType.StoredProcedure,
            ct);

    public async Task<bool> DeleteUserAsync(string globalId,string createdBy, CancellationToken ct = default)
    {
        var affected = await _db.ExecuteAsync(
            Module,
            UserAuthQueries.DeleteUser,
            new { GlobalID = globalId , CreatedBy  = createdBy },
            CommandType.StoredProcedure,
            ct);
        return affected > 0;
    }

    public Task<UserBrandInfo?> GetUserBrandInfoAsync(string globalId, CancellationToken ct = default) =>
        _db.QuerySingleOrDefaultAsync(
            Module,
            UserAuthQueries.GetUserBrandInfo,
            r =>
            {
                var queueNames = r.IsDBNull(r.GetOrdinal("AssociatedQueueNames"))
                    ? Array.Empty<string>()
                    : r.GetString(r.GetOrdinal("AssociatedQueueNames"))
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                return new UserBrandInfo(
                    BrandId: r.GetInt32(r.GetOrdinal("BrandID")),
                    BrandName: r.GetString(r.GetOrdinal("BrandName")),
                    AssociatedQueueNames: queueNames
                );
            },
            new { GlobalID = globalId },
            cancellationToken: ct);
}
