using Testcontainers.MsSql;
using Xunit;
using System.Threading.Tasks;
namespace Relay.Documentum.Integration.Tests.Fixtures;

public sealed class DocumentumDbFixture : IAsyncLifetime
{
    public MsSqlContainer Container { get; } = new MsSqlBuilder().Build();

    public string ConnectionString => Container.GetConnectionString();

    public Task InitializeAsync() => Container.StartAsync();

    public Task DisposeAsync() => Container.DisposeAsync().AsTask();
}
