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
        int? orderSeq,
        string? repPO,
        string? accountNumber,
        string? repUserName,
        string? brand,
        DateTime? orderDateFrom,
        DateTime? orderDateTo,
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

            command.Parameters.AddWithValue("@OrderSeq", (object?)orderSeq ?? DBNull.Value);
            command.Parameters.AddWithValue("@RepPO", (object?)repPO ?? DBNull.Value);
            command.Parameters.AddWithValue("@AccountNumber", (object?)accountNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@RepUserName", (object?)repUserName ?? DBNull.Value);
            command.Parameters.AddWithValue("@Brand", (object?)brand ?? DBNull.Value);
            command.Parameters.AddWithValue("@OrderDateFrom", (object?)orderDateFrom ?? DBNull.Value);
            command.Parameters.AddWithValue("@OrderDateTo", (object?)orderDateTo ?? DBNull.Value);
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
}
