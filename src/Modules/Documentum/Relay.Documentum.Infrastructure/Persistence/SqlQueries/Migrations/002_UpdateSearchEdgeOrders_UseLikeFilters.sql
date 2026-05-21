-- ============================================================================
-- Migration: Update usp_SearchEdgeOrders
--   1. Use LIKE for text column filters (partial match)
--   2. Add dynamic column sorting (@SortField / @SortDirection)
-- Date: 2026-05-20
-- ============================================================================

ALTER PROCEDURE [dbo].[usp_SearchEdgeOrders]
    @SalesOrderNumber VARCHAR(50)  = NULL,
    @RepPO            VARCHAR(50)  = NULL,
    @AccountNumber    VARCHAR(50)  = NULL,
    @ProductType      VARCHAR(50)  = NULL,
    @Region           VARCHAR(50)  = NULL,
    @Priority         VARCHAR(50)  = NULL,
    @Brand            VARCHAR(50)  = NULL,
    @CaptureDateFrom  DATETIME     = NULL,
    @CaptureDateTo    DATETIME     = NULL,
    @JobName          VARCHAR(100) = NULL,
    @QueueName        VARCHAR(100) = NULL,
    @PackageOwner     VARCHAR(100) = NULL,
    @RepName          VARCHAR(100) = NULL,
    @SortField        VARCHAR(50)  = NULL,
    @SortDirection    VARCHAR(4)   = NULL,   -- 'asc' or 'desc'
    @PageNumber       INT          = 1,
    @PageSize         INT          = 50,
    @TotalCount       INT          OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- ── Count query ─────────────────────────────────────────────────────────
    SELECT @TotalCount = COUNT(*)
    FROM dbo.EdgeOrders EO WITH (NOLOCK)
    LEFT JOIN EdgeOrderDetails EOD WITH (NOLOCK)
        ON EO.orderSeq = EOD.orderSeq AND EOD.IsActive = 1
    WHERE
        (@RepPO IS NULL OR EO.repPO LIKE '%' + @RepPO + '%')
        AND (@AccountNumber IS NULL OR EO.AccountNumber LIKE '%' + @AccountNumber + '%')
        AND (@Brand IS NULL OR EO.brand = @Brand)
        AND (@CaptureDateFrom IS NULL OR EO.orderDate >= @CaptureDateFrom)
        AND (@CaptureDateTo IS NULL OR EO.orderDate < DATEADD(DAY, 1, @CaptureDateTo))
        AND (@PackageOwner IS NULL OR EO.repUserName LIKE '%' + @PackageOwner + '%')
        AND (@RepName IS NULL OR EOD.RepName LIKE '%' + @RepName + '%')
        AND (@SalesOrderNumber IS NULL OR EOD.SalesOrderNumber LIKE '%' + @SalesOrderNumber + '%')
        AND (@Priority IS NULL OR EOD.[Priority] = @Priority)
        AND (@QueueName IS NULL OR EOD.QueueName LIKE '%' + @QueueName + '%')
        AND (@ProductType IS NULL OR EOD.ProductType LIKE '%' + @ProductType + '%')
        AND (@Region IS NULL OR EOD.Region = @Region)
        AND (@JobName IS NULL OR EOD.JobName LIKE '%' + @JobName + '%');

    -- ── Data query with dynamic ORDER BY ────────────────────────────────────
    SELECT
        EO.orderGUID,
        EO.orderSeq,
        EO.brand,
        EO.repPO,
        EO.repUserName,
        EO.AccountNumber,
        EO.orderDate,
        EO.repCustomer,
        EO.repSalesPerson,
        EO.jobNumber,
        EO.[status],
        EO.totalNet,
        EO.OrderRecdDate,
        EOD.SalesOrderNumber,
        EOD.[Priority],
        EOD.RepName,
        EOD.QueueName,
        EOD.ProductType,
        EOD.Region,
        EOD.JobName,
        EOD.CreatedDate,
        EOD.CompletionDate,
        EO.repUserName AS [PackageOwner]
    FROM dbo.EdgeOrders EO WITH (NOLOCK)
    LEFT JOIN EdgeOrderDetails EOD WITH (NOLOCK)
        ON EO.orderSeq = EOD.orderSeq AND EOD.IsActive = 1
    WHERE
        (@RepPO IS NULL OR EO.repPO LIKE '%' + @RepPO + '%')
        AND (@AccountNumber IS NULL OR EO.AccountNumber LIKE '%' + @AccountNumber + '%')
        AND (@Brand IS NULL OR EO.brand = @Brand)
        AND (@CaptureDateFrom IS NULL OR EO.orderDate >= @CaptureDateFrom)
        AND (@CaptureDateTo IS NULL OR EO.orderDate < DATEADD(DAY, 1, @CaptureDateTo))
        AND (@PackageOwner IS NULL OR EO.repUserName LIKE '%' + @PackageOwner + '%')
        AND (@RepName IS NULL OR EOD.RepName LIKE '%' + @RepName + '%')
        AND (@SalesOrderNumber IS NULL OR EOD.SalesOrderNumber LIKE '%' + @SalesOrderNumber + '%')
        AND (@Priority IS NULL OR EOD.[Priority] = @Priority)
        AND (@QueueName IS NULL OR EOD.QueueName LIKE '%' + @QueueName + '%')
        AND (@ProductType IS NULL OR EOD.ProductType LIKE '%' + @ProductType + '%')
        AND (@Region IS NULL OR EOD.Region = @Region)
        AND (@JobName IS NULL OR EOD.JobName LIKE '%' + @JobName + '%')
    ORDER BY
        -- Dynamic sort column (whitelist approach for safety)
        CASE WHEN @SortDirection = 'asc' THEN
            CASE @SortField
                WHEN 'repPO'            THEN EO.repPO
                WHEN 'accountNumber'    THEN EO.AccountNumber
                WHEN 'brand'            THEN EO.brand
                WHEN 'salesOrderNumber' THEN EOD.SalesOrderNumber
                WHEN 'priority'         THEN EOD.[Priority]
                WHEN 'repName'          THEN EOD.RepName
                WHEN 'queueName'        THEN EOD.QueueName
                WHEN 'productType'      THEN EOD.ProductType
                WHEN 'region'           THEN EOD.Region
                WHEN 'jobName'          THEN EOD.JobName
                WHEN 'packageOwner'     THEN EO.repUserName
            END
        END ASC,
        CASE WHEN @SortDirection = 'desc' THEN
            CASE @SortField
                WHEN 'repPO'            THEN EO.repPO
                WHEN 'accountNumber'    THEN EO.AccountNumber
                WHEN 'brand'            THEN EO.brand
                WHEN 'salesOrderNumber' THEN EOD.SalesOrderNumber
                WHEN 'priority'         THEN EOD.[Priority]
                WHEN 'repName'          THEN EOD.RepName
                WHEN 'queueName'        THEN EOD.QueueName
                WHEN 'productType'      THEN EOD.ProductType
                WHEN 'region'           THEN EOD.Region
                WHEN 'jobName'          THEN EOD.JobName
                WHEN 'packageOwner'     THEN EO.repUserName
            END
        END DESC,
        -- Date sort columns (separate CASE for datetime type)
        CASE WHEN @SortDirection = 'asc' THEN
            CASE @SortField
                WHEN 'createdDate' THEN EOD.CreatedDate
                WHEN 'orderDate'   THEN EO.orderDate
            END
        END ASC,
        CASE WHEN @SortDirection = 'desc' THEN
            CASE @SortField
                WHEN 'createdDate' THEN EOD.CreatedDate
                WHEN 'orderDate'   THEN EO.orderDate
            END
        END DESC,
        -- Default sort when no sort field specified
        EO.orderDate DESC,
        EO.orderSeq DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY
    OPTION (RECOMPILE);
END
