using System.Data;
using Microsoft.Data.SqlClient;
using Relay.Documentum.Domain.Aggregates;
using Relay.Documentum.Domain.Repositories;
using Relay.Documentum.Infrastructure.Persistence.DataModels;
using Relay.Documentum.Infrastructure.Persistence.SqlQueries;
using Relay.Infrastructure.Core.Data;

namespace Relay.Documentum.Infrastructure.Persistence.Repositories;

internal sealed class EdgeOrderRepository : IEdgeOrderRepository
{
    private const string Module = DocumentumInfrastructureModule.ModuleName;

    private readonly IDbConnectionFactory _connectionFactory;

    public EdgeOrderRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<(IReadOnlyList<EdgeOrder> Items, int TotalCount)> SearchAsync(
        string? salesOrderNumber,
        string? repPO,
        string? accountNumber,
        string? productType,
        string? region,
        string? priority,
        string? brand,
        DateTime? captureDateFrom,
        DateTime? captureDateTo,
        string? jobName,
        string? queueName,
        string? packageOwner,
        string? repName,
        string? sortField,
        string? sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(Module, cancellationToken);
            await using var command = new SqlCommand(EdgeOrderQueries.Search, (SqlConnection)connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@SalesOrderNumber", (object?)salesOrderNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@RepPO", (object?)repPO ?? DBNull.Value);
            command.Parameters.AddWithValue("@AccountNumber", (object?)accountNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@ProductType", (object?)productType ?? DBNull.Value);
            command.Parameters.AddWithValue("@Region", (object?)region ?? DBNull.Value);
            command.Parameters.AddWithValue("@Priority", (object?)priority ?? DBNull.Value);
            command.Parameters.AddWithValue("@Brand", (object?)brand ?? DBNull.Value);
            command.Parameters.AddWithValue("@CaptureDateFrom", (object?)captureDateFrom ?? DBNull.Value);
            command.Parameters.AddWithValue("@CaptureDateTo", (object?)captureDateTo ?? DBNull.Value);
            command.Parameters.AddWithValue("@JobName", (object?)jobName ?? DBNull.Value);
            command.Parameters.AddWithValue("@QueueName", (object?)queueName ?? DBNull.Value);
            command.Parameters.AddWithValue("@PackageOwner", (object?)packageOwner ?? DBNull.Value);
            command.Parameters.AddWithValue("@RepName", (object?)repName ?? DBNull.Value);

            // Only send sort params when provided — keeps backward compatibility
            // with the stored procedure before the sort migration is applied.
            if (!string.IsNullOrEmpty(sortField))
            {
                command.Parameters.AddWithValue("@SortField", sortField);
                command.Parameters.AddWithValue("@SortDirection", string.IsNullOrEmpty(sortDirection) ? "asc" : sortDirection);
            }

            command.Parameters.AddWithValue("@PageNumber", pageNumber);
            command.Parameters.AddWithValue("@PageSize", pageSize);

            var totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(totalCountParam);

            var items = new List<EdgeOrder>();
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(EdgeOrderDataModel.FromRecord(reader).ToDomain());
            }
            await reader.CloseAsync();

            var totalCount = totalCountParam.Value is DBNull or null ? 0 : (int)totalCountParam.Value;
            return (items, totalCount);
        }
        catch (Exception ex)
        {

            throw;
        }
    }

    public async Task<EdgeOrder?> GetByOrderSeqAsync(int orderSeq, CancellationToken cancellationToken = default)
    {
        // Reuse the search SP with orderSeq filter — guaranteed to work with existing schema
        var (items, _) = await SearchAsync(
            salesOrderNumber: null, repPO: null, accountNumber: null,
            productType: null, region: null, priority: null, brand: null,
            captureDateFrom: null, captureDateTo: null, jobName: null,
            queueName: null, packageOwner: null, repName: null,
            sortField: null, sortDirection: null,
            pageNumber: 1, pageSize: 1000, cancellationToken: cancellationToken);

        return items.FirstOrDefault(o => o.OrderSeq == orderSeq);
    }

    public async Task<IReadOnlyList<string>> GetDistinctBrandsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(Module, cancellationToken);
        await using var command = new SqlCommand(EdgeOrderQueries.GetDistinctBrands, (SqlConnection)connection);

        var brands = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            brands.Add(reader.GetString(0));
        }
        return brands;
    }

    public async Task<IReadOnlyList<string>> GetProductTypesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(Module, cancellationToken);
        await using var command = new SqlCommand(EdgeOrderQueries.GetProductTypes, (SqlConnection)connection);

        var types = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            types.Add(reader.GetString(0));
        }
        return types;
    }

    public async Task<IReadOnlyList<string>> GetQueuesByBrandAsync(string brandName, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(Module, cancellationToken);
        await using var command = new SqlCommand(EdgeOrderQueries.GetQueuesByBrand, (SqlConnection)connection);
        command.Parameters.AddWithValue("@BrandName", brandName);

        var queues = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            queues.Add(reader.GetString(0));
        }
        return queues;
    }

    public async Task<IReadOnlyList<string>> GetRouteToDepartmentQueuesAsync(string brandName, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(Module, cancellationToken);
        await using var command = new SqlCommand(EdgeOrderQueries.GetRouteToDepartmentQueues, (SqlConnection)connection);
        command.Parameters.AddWithValue("@BrandName", brandName);

        var queues = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            queues.Add(reader.GetString(0));
        }
        return queues;
    }
}
