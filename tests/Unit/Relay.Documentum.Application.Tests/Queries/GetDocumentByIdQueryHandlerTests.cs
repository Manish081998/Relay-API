using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Relay.Documentum.Application.Queries.GetDocumentById;
using Relay.Documentum.Application.Tests.Builders;
using Relay.Documentum.Domain.Repositories;
using Xunit;

namespace Relay.Documentum.Application.Tests.Queries;

public sealed class GetDocumentByIdQueryHandlerTests
{
    private readonly IDocumentRepository _repo = Substitute.For<IDocumentRepository>();
    private readonly GetDocumentByIdQueryHandler _handler;

    public GetDocumentByIdQueryHandlerTests()
    {
        _handler = new GetDocumentByIdQueryHandler(_repo);
    }

    [Fact]
    public async Task HandleAsync_returns_success_with_dto_when_document_exists()
    {
        var id = Guid.NewGuid();
        var document = DocumentBuilder.Build(id: id, title: "Annual Report");
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(document);

        var result = await _handler.HandleAsync(new GetDocumentByIdQuery(id));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(id);
        result.Value.Title.Should().Be("Annual Report");
    }

    [Fact]
    public async Task HandleAsync_returns_success_with_null_when_document_not_found()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Relay.Documentum.Domain.Aggregates.Document?)null);

        var result = await _handler.HandleAsync(new GetDocumentByIdQuery(id));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_calls_repository_with_correct_id()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Relay.Documentum.Domain.Aggregates.Document?)null);

        await _handler.HandleAsync(new GetDocumentByIdQuery(id));

        await _repo.Received(1).GetByIdAsync(id, Arg.Any<CancellationToken>());
    }
}
