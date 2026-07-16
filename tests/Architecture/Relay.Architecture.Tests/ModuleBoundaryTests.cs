using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Relay.Architecture.Tests;

/// <summary>
/// Enforces the module boundary rules defined in the architecture document.
/// Pipelines run these on every PR — a violation fails the build.
/// </summary>
public sealed class ModuleBoundaryTests
{
    // Domain assemblies
    private static readonly Assembly IntranetDomain   = typeof(Relay.Intranet.Domain.Aggregates.User).Assembly;
    private static readonly Assembly DocumentumDomain = typeof(Relay.Documentum.Domain.Aggregates.Document).Assembly;
    private static readonly Assembly WebToolDomain  = typeof(Relay.WebTool.Domain.Aggregates.Selection).Assembly;

    // Application assemblies
    private static readonly Assembly IntranetApp   = typeof(Relay.Intranet.Application.IntranetApplicationModule).Assembly;
    private static readonly Assembly DocumentumApp = typeof(Relay.Documentum.Application.DocumentumApplicationModule).Assembly;
    private static readonly Assembly WebToolApp  = typeof(Relay.WebTool.Application.WebToolApplicationModule).Assembly;

    // Infrastructure assemblies
    private static readonly Assembly IntranetInfra   = typeof(Relay.Intranet.Infrastructure.IntranetInfrastructureModule).Assembly;
    private static readonly Assembly DocumentumInfra = typeof(Relay.Documentum.Infrastructure.DocumentumInfrastructureModule).Assembly;
    private static readonly Assembly WebToolInfra  = typeof(Relay.WebTool.Infrastructure.WebToolInfrastructureModule).Assembly;

    [Fact]
    public void Intranet_Domain_does_not_depend_on_other_module_domains()
    {
        var result = Types.InAssembly(IntranetDomain)
            .Should()
            .NotHaveDependencyOnAny(
                "Relay.Documentum.Domain",
                "Relay.WebTool.Domain",
                "Relay.Documentum.Application",
                "Relay.WebTool.Application",
                "Relay.Documentum.Infrastructure",
                "Relay.WebTool.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Intranet.Domain has forbidden references: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Documentum_Domain_does_not_depend_on_other_module_domains()
    {
        var result = Types.InAssembly(DocumentumDomain)
            .Should()
            .NotHaveDependencyOnAny(
                "Relay.Intranet.Domain",
                "Relay.WebTool.Domain",
                "Relay.Intranet.Application",
                "Relay.WebTool.Application",
                "Relay.Intranet.Infrastructure",
                "Relay.WebTool.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void WebTool_Domain_does_not_depend_on_other_module_domains()
    {
        var result = Types.InAssembly(WebToolDomain)
            .Should()
            .NotHaveDependencyOnAny(
                "Relay.Intranet.Domain",
                "Relay.Documentum.Domain",
                "Relay.Intranet.Application",
                "Relay.Documentum.Application",
                "Relay.Intranet.Infrastructure",
                "Relay.Documentum.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_projects_do_not_depend_on_infrastructure_or_aspnet()
    {
        foreach (var assembly in new[] { IntranetDomain, DocumentumDomain, WebToolDomain })
        {
            var result = Types.InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny(
                    "Microsoft.AspNetCore",
                    "Microsoft.Data.SqlClient",
                    "Relay.Infrastructure.Core")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"{assembly.GetName().Name} has a forbidden reference.");
        }
    }

    [Fact]
    public void Application_projects_do_not_depend_on_other_modules_except_via_Contracts()
    {
        // Documentum.Application may reference Intranet.Contracts (documented cross-module read).
        // It may not reference Intranet.Domain / Application / Infrastructure.
        var result = Types.InAssembly(DocumentumApp)
            .Should()
            .NotHaveDependencyOnAny(
                "Relay.Intranet.Domain",
                "Relay.Intranet.Application",
                "Relay.Intranet.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_projects_do_not_reference_other_modules_infrastructure()
    {
        foreach (var assembly in new[] { IntranetInfra, DocumentumInfra, WebToolInfra })
        {
            var forbidden = new[]
            {
                "Relay.Intranet.Infrastructure",
                "Relay.Documentum.Infrastructure",
                "Relay.WebTool.Infrastructure",
            }.Where(name => name != assembly.GetName().Name).ToArray();

            var result = Types.InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny(forbidden)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"{assembly.GetName().Name} illegally references another module's infrastructure.");
        }
    }
}
