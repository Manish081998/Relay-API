using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Relay.Intranet.Application.Commands.UpdateUserByEmail;
using Relay.Intranet.Application.Tests.Builders;
using Relay.Intranet.Domain.Repositories;
using Xunit;

namespace Relay.Intranet.Application.Tests.Commands;

public sealed class UpdateUserByEmailCommandHandlerTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private readonly UpdateUserByEmailCommandHandler _handler;

    public UpdateUserByEmailCommandHandlerTests()
    {
        _handler = new UpdateUserByEmailCommandHandler(_repo);
    }

    [Fact]
    public async Task HandleAsync_returns_failure_when_user_not_found()
    {
        _repo.GetByEmailAsync("ghost@adticorp.com", Arg.Any<CancellationToken>())
            .Returns((Relay.Intranet.Domain.Aggregates.User?)null);

        var result = await _handler.HandleAsync(
            new UpdateUserByEmailCommand("ghost@adticorp.com", "Ghost User"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("User.NotFound");
    }

    [Fact]
    public async Task HandleAsync_returns_updated_dto_when_user_exists()
    {
        var user = UserBuilder.Build(displayName: "Old Name", email: "jsmith@adticorp.com");
        _repo.GetByEmailAsync("jsmith@adticorp.com", Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.HandleAsync(
            new UpdateUserByEmailCommand("jsmith@adticorp.com", "New Name"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.DisplayName.Should().Be("New Name");
        result.Value.Email.Should().Be("jsmith@adticorp.com");
    }

    [Fact]
    public async Task HandleAsync_persists_changes_to_repository()
    {
        var user = UserBuilder.Build(email: "jsmith@adticorp.com");
        _repo.GetByEmailAsync("jsmith@adticorp.com", Arg.Any<CancellationToken>()).Returns(user);

        await _handler.HandleAsync(
            new UpdateUserByEmailCommand("jsmith@adticorp.com", "Updated Name"));

        await _repo.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_does_not_call_update_when_user_not_found()
    {
        _repo.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Relay.Intranet.Domain.Aggregates.User?)null);

        await _handler.HandleAsync(
            new UpdateUserByEmailCommand("nobody@adticorp.com", "Nobody"));

        await _repo.DidNotReceive().UpdateAsync(
            Arg.Any<Relay.Intranet.Domain.Aggregates.User>(), Arg.Any<CancellationToken>());
    }
}
