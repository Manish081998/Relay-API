using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Relay.Intranet.Application.Queries.GetUserById;
using Relay.Intranet.Application.Tests.Builders;
using Relay.Intranet.Domain.Repositories;
using Xunit;

namespace Relay.Intranet.Application.Tests.Queries;

public sealed class GetUserByIdQueryHandlerTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _handler = new GetUserByIdQueryHandler(_repo);
    }

    [Fact]
    public async Task HandleAsync_returns_success_with_dto_when_user_exists()
    {
        var id = Guid.NewGuid();
        var user = UserBuilder.Build(id: id, displayName: "Jane Smith", email: "jsmith@adticorp.com");
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.HandleAsync(new GetUserByIdQuery(id));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(id);
        result.Value.DisplayName.Should().Be("Jane Smith");
        result.Value.Email.Should().Be("jsmith@adticorp.com");
    }

    [Fact]
    public async Task HandleAsync_returns_success_with_null_when_user_not_found()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Relay.Intranet.Domain.Aggregates.User?)null);

        var result = await _handler.HandleAsync(new GetUserByIdQuery(id));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_calls_repository_with_correct_id()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Relay.Intranet.Domain.Aggregates.User?)null);

        await _handler.HandleAsync(new GetUserByIdQuery(id));

        await _repo.Received(1).GetByIdAsync(id, Arg.Any<CancellationToken>());
    }
}
