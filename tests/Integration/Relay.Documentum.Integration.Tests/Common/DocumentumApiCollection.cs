using Xunit;

namespace Relay.Documentum.Integration.Tests.Common;

[CollectionDefinition(DocumentumApiCollection.Name)]
public sealed class DocumentumApiCollection : ICollectionFixture<DocumentumApiFactory>
{
    public const string Name = "Documentum API";
}
