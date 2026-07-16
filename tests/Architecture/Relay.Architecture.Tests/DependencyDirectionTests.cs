using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Relay.Architecture.Tests;

public sealed class DependencyDirectionTests
{
    [Theory]
    [InlineData("Relay.Intranet.Domain")]
    [InlineData("Relay.Documentum.Domain")]
    [InlineData("Relay.WebTool.Domain")]
    public void Domain_never_depends_on_Application_or_Infrastructure(string assemblyName)
    {
        var assembly = AppDomain.CurrentDomain
            .GetAssemblies()
            .Concat(LoadReferenced())
            .FirstOrDefault(a => a.GetName().Name == assemblyName)
            ?? Assembly.Load(assemblyName);

        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOnAny(
                assemblyName.Replace(".Domain", ".Application"),
                assemblyName.Replace(".Domain", ".Infrastructure"))
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    private static IEnumerable<Assembly> LoadReferenced()
    {
        var entry = typeof(DependencyDirectionTests).Assembly;
        foreach (var name in entry.GetReferencedAssemblies())
        {
            Assembly? loaded = null;
            try { loaded = Assembly.Load(name); }
            catch { /* ignore */ }
            if (loaded is not null) yield return loaded;
        }
    }
}
