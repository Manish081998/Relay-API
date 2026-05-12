using System.Data;
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
                
            },
            CommandType.StoredProcedure,
            ct);

    
}
