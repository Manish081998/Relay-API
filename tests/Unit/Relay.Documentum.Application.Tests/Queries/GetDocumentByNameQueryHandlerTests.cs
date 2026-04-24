using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Relay.Documentum.Application.Queries.GetDocumentByName;
using Relay.Documentum.Application.Tests.Builders;
using Relay.Documentum.Domain.Repositories;
using Xunit;

namespace Relay.Documentum.Application.Tests.Queries;

public sealed class GetDocumentByNameQueryHandlerTests
{
    private readonly IDocumentRepository _repo = Substitute.For<IDocumentRepository>();
    private readonly GetDocumentByNameQueryHandler _handler;

    public GetDocumentByNameQueryHandlerTests()
    {
        _handler = new GetDocumentByNameQueryHandler(_repo);
    }

    [Fact]
    public async Task HandleAsync_returns_failure_when_name_is_empty()
    {
        var result = await _handler.HandleAsync(new GetDocumentByNameQuery(string.Empty));

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Document.NameRequired");
    }

    [Fact]
    public async Task HandleAsync_returns_failure_when_name_is_whitespace()
    {
        var result = await _handler.HandleAsync(new GetDocumentByNameQuery("   "));

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Document.NameRequired");
    }

    [Fact]
    public async Task HandleAsync_returns_matching_documents_when_name_is_valid()
    {
        var doc = DocumentBuilder.Build(title: "Policy Manual");
        _repo.GetByNameAsync("Policy", Arg.Any<CancellationToken>())
            .Returns(new List<Relay.Documentum.Domain.Aggregates.Document> { doc });

        var result = await _handler.HandleAsync(new GetDocumentByNameQuery("Policy"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value![0].Title.Should().Be("Policy Manual");
    }

    [Fact]
    public async Task HandleAsync_returns_empty_list_when_no_documents_match()
    {
        _repo.GetByNameAsync("NoMatch", Arg.Any<CancellationToken>())
            .Returns(new List<Relay.Documentum.Domain.Aggregates.Document>());

        var result = await _handler.HandleAsync(new GetDocumentByNameQuery("NoMatch"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_does_not_call_repository_when_name_is_empty()
    {
        await _handler.HandleAsync(new GetDocumentByNameQuery(string.Empty));

        await _repo.DidNotReceive().GetByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
