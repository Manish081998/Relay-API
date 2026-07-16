using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Relay.WebTool.Application.Queries.GetSelectionById;
using Relay.WebTool.Application.Tests.Builders;
using Relay.WebTool.Domain.Repositories;
using Xunit;

namespace Relay.WebTool.Application.Tests.Queries;

public sealed class GetSelectionByIdQueryHandlerTests
{
    private readonly ISelectionRepository _repo = Substitute.For<ISelectionRepository>();
    private readonly GetSelectionByIdQueryHandler _handler;

    public GetSelectionByIdQueryHandlerTests()
    {
        _handler = new GetSelectionByIdQueryHandler(_repo);
    }

    [Fact]
    public async Task HandleAsync_returns_success_with_dto_when_selection_exists()
    {
        var id = Guid.NewGuid();
        var selection = SelectionBuilder.Build(id: id, title: "Q4 Budget Review");
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(selection);

        var result = await _handler.HandleAsync(new GetSelectionByIdQuery(id));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(id);
        result.Value.Title.Should().Be("Q4 Budget Review");
        result.Value.Options.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_returns_success_with_null_when_selection_not_found()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Relay.WebTool.Domain.Aggregates.Selection?)null);

        var result = await _handler.HandleAsync(new GetSelectionByIdQuery(id));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_calls_repository_with_correct_id()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Relay.WebTool.Domain.Aggregates.Selection?)null);

        await _handler.HandleAsync(new GetSelectionByIdQuery(id));

        await _repo.Received(1).GetByIdAsync(id, Arg.Any<CancellationToken>());
    }
}
