namespace Relay.Documentum.Infrastructure.Persistence.SqlQueries;

internal static class QueueQueries
{
    public const string GetAll = @"
        SELECT
            QueueId,
            QueueName,
            Description,
            IsActive,
            CreatedBy,
            CreatedDate,
            ModifiedBy,
            ModifiedDate
        FROM dbo.QueueMaster";

    public const string Insert = @"
        INSERT INTO dbo.QueueMaster (QueueName, Description, IsActive, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
        VALUES (@QueueName, @Description, @IsActive, @CreatedBy, GETDATE(), @ModifiedBy, GETDATE());
        SELECT CAST(SCOPE_IDENTITY() AS INT)";

    public const string Update = @"
        UPDATE dbo.QueueMaster
        SET
            QueueName    = @QueueName,
            Description  = @Description,
            IsActive     = @IsActive,
            ModifiedBy   = @ModifiedBy,
            ModifiedDate = GETDATE()
        WHERE QueueId = @QueueId";

    public const string Delete = @"
        DELETE FROM dbo.QueueMaster
        WHERE QueueId = @QueueId";

    public const string GetBrandQueueMapping = "dbo.usp_GetQueuesByBrandWithMapping";
}
