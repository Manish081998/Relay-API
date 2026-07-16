using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Relay.Architecture.Tests;

public sealed class NamingConventionTests
{
    [Theory]
    [InlineData("Relay.Intranet.Application")]
    [InlineData("Relay.Documentum.Application")]
    [InlineData("Relay.WebTool.Application")]
    public void Command_handlers_end_with_CommandHandler(string assemblyName)
    {
        var assembly = LoadAssembly(assemblyName);

        var result = Types.InAssembly(assembly)
            .That()
            .ImplementInterface(typeof(Relay.SharedKernel.Application.ICommandHandler<,>))
            .Or()
            .ImplementInterface(typeof(Relay.SharedKernel.Application.ICommandHandler<>))
            .Should()
            .HaveNameEndingWith("CommandHandler")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Command handlers must end with 'CommandHandler'. Offenders: {0}",
            string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Theory]
    [InlineData("Relay.Intranet.Application")]
    [InlineData("Relay.Documentum.Application")]
    [InlineData("Relay.WebTool.Application")]
    public void Query_handlers_end_with_QueryHandler(string assemblyName)
    {
        var assembly = LoadAssembly(assemblyName);

        var result = Types.InAssembly(assembly)
            .That()
            .ImplementInterface(typeof(Relay.SharedKernel.Application.IQueryHandler<,>))
            .Should()
            .HaveNameEndingWith("QueryHandler")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Query handlers must end with 'QueryHandler'. Offenders: {0}",
            string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Theory]
    [InlineData("Relay.Intranet.Application")]
    [InlineData("Relay.Documentum.Application")]
    [InlineData("Relay.WebTool.Application")]
    public void Validators_end_with_Validator(string assemblyName)
    {
        var assembly = LoadAssembly(assemblyName);

        var result = Types.InAssembly(assembly)
            .That()
            .Inherit(typeof(FluentValidation.AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Validators must end with 'Validator'. Offenders: {0}",
            string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Theory]
    [InlineData("Relay.Intranet.Infrastructure")]
    [InlineData("Relay.Documentum.Infrastructure")]
    [InlineData("Relay.WebTool.Infrastructure")]
    public void Repositories_end_with_Repository(string assemblyName)
    {
        var assembly = LoadAssembly(assemblyName);

        var result = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .BeClasses()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Types ending with 'Repository' must be concrete classes. Offenders: {0}",
            string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Theory]
    [InlineData("Relay.Intranet.Domain")]
    [InlineData("Relay.Documentum.Domain")]
    [InlineData("Relay.WebTool.Domain")]
    public void Domain_events_end_with_DomainEvent(string assemblyName)
    {
        var assembly = LoadAssembly(assemblyName);

        var result = Types.InAssembly(assembly)
            .That()
            .ImplementInterface(typeof(Relay.SharedKernel.Domain.IDomainEvent))
            .Should()
            .HaveNameEndingWith("DomainEvent")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Domain events must end with 'DomainEvent'. Offenders: {0}",
            string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    private static Assembly LoadAssembly(string assemblyName)
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Concat(LoadReferenced())
            .FirstOrDefault(a => a.GetName().Name == assemblyName)
            ?? Assembly.Load(assemblyName);
    }

    private static IEnumerable<Assembly> LoadReferenced()
    {
        var entry = typeof(NamingConventionTests).Assembly;
        foreach (var name in entry.GetReferencedAssemblies())
        {
            Assembly? loaded = null;
            try { loaded = Assembly.Load(name); }
            catch { /* ignore */ }
            if (loaded is not null) yield return loaded;
        }
    }
}
