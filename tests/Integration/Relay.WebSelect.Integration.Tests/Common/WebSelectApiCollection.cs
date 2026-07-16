using Xunit;

namespace Relay.WebTool.Integration.Tests.Common;

[CollectionDefinition(WebToolApiCollection.Name)]
public sealed class WebToolApiCollection : ICollectionFixture<WebToolApiFactory>
{
    public const string Name = "WebTool API";
}
