using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Relay.SharedKernel.Application;

namespace Relay.Intranet.Integration.Tests.Common;

public sealed class IntranetApiFactory : WebApplicationFactory<Program>
{
    public IQueryDispatcher QueryDispatcher { get; } = Substitute.For<IQueryDispatcher>();
    public ICommandDispatcher CommandDispatcher { get; } = Substitute.For<ICommandDispatcher>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IQueryDispatcher>();
            services.AddSingleton(QueryDispatcher);
            services.RemoveAll<ICommandDispatcher>();
            services.AddSingleton(CommandDispatcher);
        });
    }
}
