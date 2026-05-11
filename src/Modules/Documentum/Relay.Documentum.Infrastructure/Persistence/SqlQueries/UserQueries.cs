namespace Relay.Documentum.Infrastructure.Persistence.SqlQueries;

internal static class UserQueries
{
    public const string GetAll = @"
        SELECT
            U.UserId,
            U.GlobalId,
            U.Password,
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
        INSERT INTO UserMaster (UserId, GlobalID, Password, FirstName, LastName, EmailID, BrandID, IsActive, CreatedBy, CreatedDate)
        VALUES (@UserId, @GlobalId, @Password, @FirstName, @LastName, @EmailId, @BrandId, @IsActive, @CreatedBy, GETDATE())";

    public const string Update = @"
        UPDATE UserMaster
        SET 
            BrandID     = @BrandId,
            IsActive    = @IsActive,
            ModifiedBy  = @ModifiedBy,
            ModifiedDate = GETDATE()
        WHERE UserId = @UserId";
}
