using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Relay.Documentum.Application.Commands.UpdateDocumentById;
using Relay.Documentum.Application.Tests.Builders;
using Relay.Documentum.Domain.Repositories;
using Xunit;

namespace Relay.Documentum.Application.Tests.Commands;

public sealed class UpdateDocumentByIdCommandHandlerTests
{
    private readonly IDocumentRepository _repo = Substitute.For<IDocumentRepository>();
    private readonly UpdateDocumentByIdCommandHandler _handler;

    public UpdateDocumentByIdCommandHandlerTests()
    {
        _handler = new UpdateDocumentByIdCommandHandler(
            _repo,
            Substitute.For<ILogger<UpdateDocumentByIdCommandHandler>>());
    }

    [Fact]
    public async Task HandleAsync_returns_failure_when_document_not_found()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Relay.Documentum.Domain.Aggregates.Document?)null);

        var result = await _handler.HandleAsync(
            new UpdateDocumentByIdCommand(id, "New Title", "/docs/new.pdf", 2048));

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Document.NotFound");
    }

    [Fact]
    public async Task HandleAsync_returns_updated_dto_when_document_exists()
    {
        var id = Guid.NewGuid();
        var document = DocumentBuilder.Build(id: id, title: "Old Title");
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(document);

        var result = await _handler.HandleAsync(
            new UpdateDocumentByIdCommand(id, "New Title", "/docs/updated.pdf", 4096));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("New Title");
        result.Value.SizeInBytes.Should().Be(4096);
    }

    [Fact]
    public async Task HandleAsync_persists_changes_to_repository()
    {
        var id = Guid.NewGuid();
        var document = DocumentBuilder.Build(id: id);
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(document);

        await _handler.HandleAsync(
            new UpdateDocumentByIdCommand(id, "Updated", "/docs/updated.pdf", 512));

        await _repo.Received(1).UpdateAsync(document, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_does_not_call_update_when_document_not_found()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Relay.Documentum.Domain.Aggregates.Document?)null);

        await _handler.HandleAsync(
            new UpdateDocumentByIdCommand(id, "Title", "/docs/file.pdf", 1024));

        await _repo.DidNotReceive().UpdateAsync(
            Arg.Any<Relay.Documentum.Domain.Aggregates.Document>(), Arg.Any<CancellationToken>());
    }
}
