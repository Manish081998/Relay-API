using Xunit;

namespace Relay.Intranet.Integration.Tests.Common;

[CollectionDefinition(IntranetApiCollection.Name)]
public sealed class IntranetApiCollection : ICollectionFixture<IntranetApiFactory>
{
    public const string Name = "Intranet API";
}
