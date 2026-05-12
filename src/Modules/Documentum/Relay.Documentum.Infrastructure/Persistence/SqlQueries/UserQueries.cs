namespace Relay.Documentum.Infrastructure.Persistence.SqlQueries;

internal static class UserQueries
{
    public const string GetAll = @"
        SELECT
            U.UserId,
            U.GlobalId,
	        U.EmailId,
            U.BrandId,
            B.BrandName,
            U.FirstName,
            U.LastName,
            U.IsActive,
            U.CreatedBy,
            U.CreatedDate,
            U.ModifiedBy,
            U.ModifiedDate
        FROM UserMaster U
        INNER JOIN BrandMaster B ON U.BrandId = B.BrandId";

    public const string Insert = @"
        INSERT INTO UserMaster (GlobalID, FirstName, LastName, EmailID, BrandID, IsActive, CreatedBy, CreatedDate)
        VALUES (@GlobalId, @FirstName, @LastName, @EmailId, @BrandId, @IsActive, @CreatedBy, GETDATE());
        SELECT CAST(SCOPE_IDENTITY() AS INT)";

    public const string Update = @"
        UPDATE UserMaster
        SET 
            BrandID     = @BrandId,
            IsActive    = @IsActive,
            ModifiedBy  = @ModifiedBy,
            ModifiedDate = GETDATE()
        WHERE UserId = @UserId";
}
