USE [OrderManagement]
GO

/****** Object:  StoredProcedure [dbo].[usp_SearchEdgeOrders]    Script Date: 6/10/2026 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- ============================================================================
-- Updated to pull Queue Name from EdgeOrderState -> QueueMaster (live workflow state)
-- and Date In Queue from EdgeOrderHistory (most recent arrival into current queue)
-- Falls back to EdgeOrderDetails.QueueName if no workflow state exists
-- ============================================================================
ALTER PROCEDURE [dbo].[usp_SearchEdgeOrders]
    @SalesOrderNumber VARCHAR(50) = NULL
    ,@RepPO VARCHAR(50) = NULL
    ,@AccountNumber VARCHAR(50) = NULL
    ,@ProductType VARCHAR(50) = NULL
    ,@Region VARCHAR(50) = NULL
    ,@Priority VARCHAR(50) = NULL
    ,@Brand VARCHAR(50) = NULL
    ,@CaptureDateFrom DATETIME = NULL
    ,@CaptureDateTo DATETIME = NULL
    ,@JobName VARCHAR(100) = NULL
    ,@QueueName VARCHAR(100) = NULL
    ,@PackageOwner VARCHAR(100) = NULL
    ,@RepName VARCHAR(100) = NULL
    ,@SortField VARCHAR(50) = NULL
    ,@SortDirection VARCHAR(4) = NULL
    ,@PageNumber INT = 1
    ,@PageSize INT = 50
    ,@TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- ── Count query ─────────────────────────────────────────────────────────
    SELECT @TotalCount = COUNT(*)
    FROM dbo.EdgeOrders EO WITH (NOLOCK)
    LEFT JOIN EdgeOrderDetails EOD WITH (NOLOCK)
        ON EO.orderSeq = EOD.orderSeq AND EOD.IsActive = 1
    LEFT JOIN dbo.EdgeOrderState EOS WITH (NOLOCK)
        ON EO.orderSeq = EOS.OrderSeq
    LEFT JOIN dbo.QueueMaster QM WITH (NOLOCK)
        ON QM.QueueId = EOS.CurrentQueueID
    WHERE (
            @RepPO IS NULL
            OR EO.repPO LIKE '%' + @RepPO + '%'
            )
        AND (
            @AccountNumber IS NULL
            OR EO.AccountNumber LIKE '%' + @AccountNumber + '%'
            )
        AND (
            @Brand IS NULL
            OR EO.brand = @Brand
            )
        AND (
            @CaptureDateFrom IS NULL
            OR EO.orderDate >= @CaptureDateFrom
            )
        AND (
            @CaptureDateTo IS NULL
            OR EO.orderDate < DATEADD(DAY, 1, @CaptureDateTo)
            )
        AND (
            @PackageOwner IS NULL
            OR EOD.PackageOwner LIKE '%' + @PackageOwner + '%'
            )
        AND (
            @RepName IS NULL
            OR EOD.RepName LIKE '%' + @RepName + '%'
            )
        AND (
            @SalesOrderNumber IS NULL
            OR EOD.SalesOrderNumber LIKE '%' + @SalesOrderNumber + '%'
            )
        AND (
            @Priority IS NULL
            OR EOD.[Priority] = @Priority
            )
        AND (
            @QueueName IS NULL
            OR COALESCE(QM.QueueName, EOD.QueueName) LIKE '%' + @QueueName + '%'
            )
        AND (
            @ProductType IS NULL
            OR EOD.ProductType LIKE '%' + @ProductType + '%'
            )
        AND (
            @Region IS NULL
            OR EOD.Region = @Region
            )
        AND (
            @JobName IS NULL
            OR EOD.JobName LIKE '%' + @JobName + '%'
            );

    -- ── Data query with dynamic ORDER BY ────────────────────────────────────
    SELECT EO.orderGUID
        ,EO.orderSeq
        ,EO.brand
        ,EO.repPO
        ,EO.repUserName
        ,EO.AccountNumber
        ,EO.orderDate AS CreatedDate
        -- DateInQueue: most recent arrival into current queue (same as StartedOn in workflow)
        ,ISNULL(
            DIQ.DateInQueue,
            EOS.CreatedDate
        ) AS orderDate
        ,EO.repCustomer
        ,EO.repSalesPerson
        ,EO.jobNumber
        ,EO.[status]
        ,EO.totalNet
        ,EO.OrderRecdDate
        ,EOD.SalesOrderNumber
        ,EOD.[Priority]
        ,EOD.RepName
        -- QueueName: live workflow state queue, fallback to EdgeOrderDetails
        ,COALESCE(QM.QueueName, EOD.QueueName) AS QueueName
        ,EOD.ProductType
        ,EOD.Region
        ,EOD.JobName
        ,EOD.CompletionDate
        ,EOD.PackageOwner AS [PackageOwner]
    FROM dbo.EdgeOrders EO WITH (NOLOCK)
    LEFT JOIN EdgeOrderDetails EOD WITH (NOLOCK)
        ON EO.orderSeq = EOD.orderSeq AND EOD.IsActive = 1
    LEFT JOIN dbo.EdgeOrderState EOS WITH (NOLOCK)
        ON EO.orderSeq = EOS.OrderSeq
    LEFT JOIN dbo.QueueMaster QM WITH (NOLOCK)
        ON QM.QueueId = EOS.CurrentQueueID
    -- DateInQueue: most recent Routing/Creation entry into current queue
    OUTER APPLY (
        SELECT TOP 1 h.StatusChangeDate AS DateInQueue
        FROM dbo.EdgeOrderHistory h WITH (NOLOCK)
        WHERE h.EdgeOrderStateId = EOS.EdgeOrderStateId
          AND h.CurrentQueueID = EOS.CurrentQueueID
          AND h.WorkflowActionStatusId IN (
              SELECT ots.OrderTransitionId
              FROM dbo.OrderTransitionStatus ots
              WHERE ots.Category = 'WorkflowAction'
                AND ots.Status IN ('Routing', 'Creation')
          )
        ORDER BY h.StatusChangeDate DESC
    ) DIQ
    WHERE (
            @RepPO IS NULL
            OR EO.repPO LIKE '%' + @RepPO + '%'
            )
        AND (
            @AccountNumber IS NULL
            OR EO.AccountNumber LIKE '%' + @AccountNumber + '%'
            )
        AND (
            @Brand IS NULL
            OR EO.brand = @Brand
            )
        AND (
            @CaptureDateFrom IS NULL
            OR EO.orderDate >= @CaptureDateFrom
            )
        AND (
            @CaptureDateTo IS NULL
            OR EO.orderDate < DATEADD(DAY, 1, @CaptureDateTo)
            )
        AND (
            @PackageOwner IS NULL
            OR EOD.PackageOwner LIKE '%' + @PackageOwner + '%'
            )
        AND (
            @RepName IS NULL
            OR EOD.RepName LIKE '%' + @RepName + '%'
            )
        AND (
            @SalesOrderNumber IS NULL
            OR EOD.SalesOrderNumber LIKE '%' + @SalesOrderNumber + '%'
            )
        AND (
            @Priority IS NULL
            OR EOD.[Priority] = @Priority
            )
        AND (
            @QueueName IS NULL
            OR COALESCE(QM.QueueName, EOD.QueueName) LIKE '%' + @QueueName + '%'
            )
        AND (
            @ProductType IS NULL
            OR EOD.ProductType LIKE '%' + @ProductType + '%'
            )
        AND (
            @Region IS NULL
            OR EOD.Region = @Region
            )
        AND (
            @JobName IS NULL
            OR EOD.JobName LIKE '%' + @JobName + '%'
            )
    ORDER BY CASE
            WHEN @SortDirection = 'asc'
                THEN CASE @SortField
                        WHEN 'repPO'
                            THEN EO.repPO
                        WHEN 'accountNumber'
                            THEN EO.AccountNumber
                        WHEN 'brand'
                            THEN EO.brand
                        WHEN 'salesOrderNumber'
                            THEN EOD.SalesOrderNumber
                        WHEN 'priority'
                            THEN EOD.[Priority]
                        WHEN 'repName'
                            THEN EOD.RepName
                        WHEN 'queueName'
                            THEN COALESCE(QM.QueueName, EOD.QueueName)
                        WHEN 'productType'
                            THEN EOD.ProductType
                        WHEN 'region'
                            THEN EOD.Region
                        WHEN 'jobName'
                            THEN EOD.JobName
                        WHEN 'packageOwner'
                            THEN EOD.PackageOwner
                        END
            END ASC
        ,CASE
            WHEN @SortDirection = 'desc'
                THEN CASE @SortField
                        WHEN 'repPO'
                            THEN EO.repPO
                        WHEN 'accountNumber'
                            THEN EO.AccountNumber
                        WHEN 'brand'
                            THEN EO.brand
                        WHEN 'salesOrderNumber'
                            THEN EOD.SalesOrderNumber
                        WHEN 'priority'
                            THEN EOD.[Priority]
                        WHEN 'repName'
                            THEN EOD.RepName
                        WHEN 'queueName'
                            THEN COALESCE(QM.QueueName, EOD.QueueName)
                        WHEN 'productType'
                            THEN EOD.ProductType
                        WHEN 'region'
                            THEN EOD.Region
                        WHEN 'jobName'
                            THEN EOD.JobName
                        WHEN 'packageOwner'
                            THEN EOD.PackageOwner
                        END
            END DESC
        ,
        -- Date sort columns (separate CASE for datetime type)
        CASE
            WHEN @SortDirection = 'asc'
                THEN CASE @SortField
                        WHEN 'createdDate'
                            THEN EOD.CreatedDate
                        WHEN 'completionDate'
                            THEN EOD.CompletionDate
                        WHEN 'orderDate'
                            THEN ISNULL(DIQ.DateInQueue, EOS.CreatedDate)
                        END
            END ASC
        ,CASE
            WHEN @SortDirection = 'desc'
                THEN CASE @SortField
                        WHEN 'createdDate'
                            THEN EOD.CreatedDate
                        WHEN 'completionDate'
                            THEN EOD.CompletionDate
                        WHEN 'orderDate'
                            THEN ISNULL(DIQ.DateInQueue, EOS.CreatedDate)
                        END
            END DESC
        ,
        -- Default sort when no sort field specified
        EO.orderDate DESC
        ,EO.orderSeq DESC OFFSET(@PageNumber - 1) * @PageSize ROWS

    FETCH NEXT @PageSize ROWS ONLY
    OPTION (RECOMPILE);
END
GO
