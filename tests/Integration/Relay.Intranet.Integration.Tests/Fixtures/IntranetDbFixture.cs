using Testcontainers.MsSql;
using Xunit;
using System.Threading.Tasks;
namespace Relay.Intranet.Integration.Tests.Fixtures;

/// <summary>
/// Spins up a disposable SQL Server container for the test class. Uses Testcontainers,
/// so the developer does not need a local SQL Server — just Docker.
/// </summary>
public sealed class IntranetDbFixture : IAsyncLifetime
{
    public MsSqlContainer Container { get; } = new MsSqlBuilder().Build();

    public string ConnectionString => Container.GetConnectionString();

    public Task InitializeAsync() => Container.StartAsync();

    public Task DisposeAsync() => Container.DisposeAsync().AsTask();
}
