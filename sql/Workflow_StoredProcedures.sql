-- ============================================================================
-- WORKFLOW STORED PROCEDURES
-- Tables: EdgeOrderState, EdgeOrderHistory, QueueMaster, OrderTransitionStatus
-- ============================================================================

-- ============================================================================
-- 1. usp_ProcessWorkflowAction
--    Single SP for Acquire (1), Unassign (2), Complete/Route (3)
-- ============================================================================
IF OBJECT_ID('dbo.usp_ProcessWorkflowAction', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_ProcessWorkflowAction;
GO

CREATE PROCEDURE dbo.usp_ProcessWorkflowAction
    @OrderSeq           INT,
    @ActionFlag         INT,            -- 1=Acquire, 2=Unassign, 3=Complete
    @UserGlobalId       VARCHAR(50),
    @DestinationQueueId INT      = NULL,-- Required only for Complete (ActionFlag=3)
    @Comment            NVARCHAR(MAX) = NULL,
    @StatusCode         INT      OUTPUT,-- 0=Success, 1=Error
    @StatusMessage      NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EdgeOrderStateId   BIGINT;
    DECLARE @CurrentQueueID     INT;
    DECLARE @IsAcquired         BIT;
    DECLARE @AcquiredBy         VARCHAR(50);
    DECLARE @Now                DATETIME = GETDATE();

    -- ── Resolve OrderTransitionStatus IDs by name (no hardcoded IDs) ────────
    DECLARE @ActionId_Acquire   INT;
    DECLARE @ActionId_Unassign  INT;
    DECLARE @ActionId_Routing   INT;
    DECLARE @StageId_Acquired   INT;
    DECLARE @StageId_Unassigned INT;
    DECLARE @StageId_Routed     INT;

    SELECT @ActionId_Acquire  = OrderTransitionId FROM dbo.OrderTransitionStatus WHERE Category = 'WorkflowAction' AND Status = 'Accuire';
    SELECT @ActionId_Unassign = OrderTransitionId FROM dbo.OrderTransitionStatus WHERE Category = 'WorkflowAction' AND Status = 'Unassign';
    SELECT @ActionId_Routing  = OrderTransitionId FROM dbo.OrderTransitionStatus WHERE Category = 'WorkflowAction' AND Status = 'Routing';
    SELECT @StageId_Acquired  = OrderTransitionId FROM dbo.OrderTransitionStatus WHERE Category = 'WorkflowStage'  AND Status = 'Order Acquired';
    SELECT @StageId_Unassigned= OrderTransitionId FROM dbo.OrderTransitionStatus WHERE Category = 'WorkflowStage'  AND Status = 'Order Unassigned';
    SELECT @StageId_Routed    = OrderTransitionId FROM dbo.OrderTransitionStatus WHERE Category = 'WorkflowStage'  AND Status = 'Order Routed';

    -- ── Fetch current state ─────────────────────────────────────────────────
    SELECT TOP 1
        @EdgeOrderStateId = EdgeOrderStateId,
        @CurrentQueueID   = CurrentQueueID,
        @IsAcquired       = IsAcquired,
        @AcquiredBy       = AcquiredBy
    FROM dbo.EdgeOrderState
    WHERE OrderSeq = @OrderSeq;

    IF @EdgeOrderStateId IS NULL
    BEGIN
        SET @StatusCode = 1;
        SET @StatusMessage = 'No workflow state found for this order.';
        RETURN;
    END

    -- ════════════════════════════════════════════════════════════════════════
    -- ACTION 1: ACQUIRE
    -- ════════════════════════════════════════════════════════════════════════
    IF @ActionFlag = 1
    BEGIN
        -- Validate: must not already be acquired
        IF @IsAcquired = 1
        BEGIN
            SET @StatusCode = 1;
            SET @StatusMessage = 'This task is already acquired by ' + ISNULL(@AcquiredBy, 'another user') + '.';
            RETURN;
        END

        BEGIN TRANSACTION;
        BEGIN TRY
            -- Insert history: Accuire action + Order Acquired stage
            INSERT INTO dbo.EdgeOrderHistory
                (EdgeOrderStateId, WorkflowActionStatusId, WorkflowStageStatusId,
                 SourceQueueId, CurrentQueueID, StatusChangeDate, StatusChangeBy,
                 Comments, CreatedBy, CreatedDate)
            VALUES
                (@EdgeOrderStateId, @ActionId_Acquire, @StageId_Acquired,
                 @CurrentQueueID, @CurrentQueueID, @Now, @UserGlobalId,
                 @Comment, @UserGlobalId, @Now);

            -- Update state
            UPDATE dbo.EdgeOrderState
            SET CurrentQueueID   = @CurrentQueueID,
                StageChangeDate  = @Now,
                StageChangeBy    = @UserGlobalId,
                IsAcquired       = 1,
                AcquiredBy       = @UserGlobalId,
                CompletionDate   = NULL,
                ModifiedBy       = @UserGlobalId,
                ModifiedDate     = @Now
            WHERE EdgeOrderStateId = @EdgeOrderStateId;

            COMMIT TRANSACTION;
            SET @StatusCode = 0;
            SET @StatusMessage = 'Task acquired successfully.';
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
            SET @StatusCode = 1;
            SET @StatusMessage = 'Failed to acquire task: ' + ERROR_MESSAGE();
        END CATCH

        RETURN;
    END

    -- ════════════════════════════════════════════════════════════════════════
    -- ACTION 2: UNASSIGN
    -- ════════════════════════════════════════════════════════════════════════
    IF @ActionFlag = 2
    BEGIN
        -- Validate: must be acquired
        IF @IsAcquired = 0
        BEGIN
            SET @StatusCode = 1;
            SET @StatusMessage = 'This task is not currently acquired.';
            RETURN;
        END

        -- Validate: only the acquiring user can unassign
        IF LOWER(@AcquiredBy) <> LOWER(@UserGlobalId)
        BEGIN
            SET @StatusCode = 1;
            SET @StatusMessage = 'Only the user who acquired this task (' + ISNULL(@AcquiredBy, '') + ') can unassign it.';
            RETURN;
        END

        BEGIN TRANSACTION;
        BEGIN TRY
            -- Insert history: Unassign action + Order Unassigned stage
            INSERT INTO dbo.EdgeOrderHistory
                (EdgeOrderStateId, WorkflowActionStatusId, WorkflowStageStatusId,
                 SourceQueueId, CurrentQueueID, StatusChangeDate, StatusChangeBy,
                 Comments, CreatedBy, CreatedDate)
            VALUES
                (@EdgeOrderStateId, @ActionId_Unassign, @StageId_Unassigned,
                 @CurrentQueueID, @CurrentQueueID, @Now, @UserGlobalId,
                 @Comment, @UserGlobalId, @Now);

            -- Update state
            UPDATE dbo.EdgeOrderState
            SET StageChangeDate  = @Now,
                StageChangeBy    = @UserGlobalId,
                IsAcquired       = 0,
                AcquiredBy       = NULL,
                CompletionDate   = NULL,
                ModifiedBy       = @UserGlobalId,
                ModifiedDate     = @Now
            WHERE EdgeOrderStateId = @EdgeOrderStateId;

            COMMIT TRANSACTION;
            SET @StatusCode = 0;
            SET @StatusMessage = 'Task unassigned successfully.';
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
            SET @StatusCode = 1;
            SET @StatusMessage = 'Failed to unassign task: ' + ERROR_MESSAGE();
        END CATCH

        RETURN;
    END

    -- ════════════════════════════════════════════════════════════════════════
    -- ACTION 3: COMPLETE / ROUTE
    -- ════════════════════════════════════════════════════════════════════════
    IF @ActionFlag = 3
    BEGIN
        -- Validate: must be acquired
        IF @IsAcquired = 0
        BEGIN
            SET @StatusCode = 1;
            SET @StatusMessage = 'This task must be acquired before it can be completed.';
            RETURN;
        END

        -- Validate: only the acquiring user can complete
        IF LOWER(@AcquiredBy) <> LOWER(@UserGlobalId)
        BEGIN
            SET @StatusCode = 1;
            SET @StatusMessage = 'Only the user who acquired this task (' + ISNULL(@AcquiredBy, '') + ') can complete it.';
            RETURN;
        END

        -- Validate: destination queue is required
        IF @DestinationQueueId IS NULL
        BEGIN
            SET @StatusCode = 1;
            SET @StatusMessage = 'A destination queue must be selected to complete/route this task.';
            RETURN;
        END

        BEGIN TRANSACTION;
        BEGIN TRY
            -- Insert history: Routing action + Order Routed stage
            INSERT INTO dbo.EdgeOrderHistory
                (EdgeOrderStateId, WorkflowActionStatusId, WorkflowStageStatusId,
                 SourceQueueId, CurrentQueueID, StatusChangeDate, StatusChangeBy,
                 Comments, CreatedBy, CreatedDate)
            VALUES
                (@EdgeOrderStateId, @ActionId_Routing, @StageId_Routed,
                 @CurrentQueueID, @DestinationQueueId, @Now, @UserGlobalId,
                 @Comment, @UserGlobalId, @Now);

            -- Update state: move to destination queue, release acquisition
            UPDATE dbo.EdgeOrderState
            SET CurrentQueueID   = @DestinationQueueId,
                StageChangeDate  = @Now,
                StageChangeBy    = @UserGlobalId,
                IsAcquired       = 0,
                AcquiredBy       = NULL,
                CompletionDate   = NULL,
                ModifiedBy       = @UserGlobalId,
                ModifiedDate     = @Now
            WHERE EdgeOrderStateId = @EdgeOrderStateId;

            COMMIT TRANSACTION;
            SET @StatusCode = 0;
            SET @StatusMessage = 'Task completed and routed successfully.';
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
            SET @StatusCode = 1;
            SET @StatusMessage = 'Failed to complete task: ' + ERROR_MESSAGE();
        END CATCH

        RETURN;
    END

    -- Invalid action flag
    SET @StatusCode = 1;
    SET @StatusMessage = 'Invalid action flag: ' + CAST(@ActionFlag AS VARCHAR(10));
END
GO


-- ============================================================================
-- 2. usp_GetWorkflowState
--    Returns current workflow state for an order
-- ============================================================================
IF OBJECT_ID('dbo.usp_GetWorkflowState', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetWorkflowState;
GO

CREATE PROCEDURE dbo.usp_GetWorkflowState
    @OrderSeq INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.EdgeOrderStateId,
        s.OrderSeq,
        s.CurrentQueueID   AS CurrentQueueId,
        q.QueueName,
        s.IsAcquired,
        s.AcquiredBy,
        ISNULL(NULLIF(LTRIM(RTRIM(ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, ''))), ''), s.AcquiredBy) AS AcquiredByName,
        s.StageChangeDate,
        s.CompletionDate,
        s.CreatedDate,
        -- StartedOn = when the job most recently arrived in the current queue
        ISNULL(
            (SELECT TOP 1 h.StatusChangeDate
             FROM dbo.EdgeOrderHistory h
             WHERE h.EdgeOrderStateId = s.EdgeOrderStateId
               AND h.CurrentQueueID = s.CurrentQueueID
               AND h.WorkflowActionStatusId IN (
                   SELECT ots.OrderTransitionId FROM dbo.OrderTransitionStatus ots
                   WHERE ots.Category = 'WorkflowAction' AND ots.Status IN ('Routing', 'Creation')
               )
             ORDER BY h.StatusChangeDate DESC),
            s.CreatedDate
        ) AS StartedOn
    FROM dbo.EdgeOrderState s
    LEFT JOIN dbo.QueueMaster q ON q.QueueId = s.CurrentQueueID
    LEFT JOIN dbo.UserMaster u ON u.GlobalID = s.AcquiredBy
    WHERE s.OrderSeq = @OrderSeq;
END
GO


-- ============================================================================
-- 3. usp_GetWorkflowHistory
--    Returns full workflow history for an order with lookups
-- ============================================================================
IF OBJECT_ID('dbo.usp_GetWorkflowHistory', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetWorkflowHistory;
GO

CREATE PROCEDURE dbo.usp_GetWorkflowHistory
    @OrderSeq INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        q.QueueName                AS ActivityName,
        ISNULL(h.Comments, '')     AS Comments,
        ISNULL(LTRIM(RTRIM(ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, ''))), h.StatusChangeBy) AS UserName,
        h.StatusChangeDate         AS [Timestamp],
        ISNULL(wa.Status, '')      AS EventType,
        ws.Status                  AS OrderStatus
    FROM dbo.EdgeOrderHistory h
    INNER JOIN dbo.EdgeOrderState s
        ON s.EdgeOrderStateId = h.EdgeOrderStateId
    LEFT JOIN dbo.QueueMaster q
        ON q.QueueId = h.SourceQueueId
    LEFT JOIN dbo.UserMaster u
        ON u.GlobalID = h.StatusChangeBy
    LEFT JOIN dbo.OrderTransitionStatus wa
        ON wa.OrderTransitionId = h.WorkflowActionStatusId
        AND wa.Category = 'WorkflowAction'
    LEFT JOIN dbo.OrderTransitionStatus ws
        ON ws.OrderTransitionId = h.WorkflowStageStatusId
        AND ws.Category = 'WorkflowStage'
    WHERE s.OrderSeq = @OrderSeq
    ORDER BY h.StatusChangeDate ASC, h.EdgeOrderHistoryId ASC;
END
GO
