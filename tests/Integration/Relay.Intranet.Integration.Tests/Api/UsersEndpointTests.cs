using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Relay.Intranet.Application.Commands.UpdateUserByEmail;
using Relay.Intranet.Application.Queries.GetUserById;
using Relay.Intranet.Contracts.Dtos;
using Relay.Intranet.Integration.Tests.Common;
using Relay.SharedKernel.Application;
using Xunit;

namespace Relay.Intranet.Integration.Tests.Api;

[Collection(IntranetApiCollection.Name)]
public sealed class UsersEndpointTests
{
    private readonly IntranetApiFactory _factory;
    private readonly HttpClient _client;

    public UsersEndpointTests(IntranetApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        factory.QueryDispatcher.ClearSubstitute();
        factory.CommandDispatcher.ClearSubstitute();
    }

    // ── GET api/intranet/users/{id} ───────────────────────────────────────────

    [Fact]
    public async Task GetById_returns_200_with_user_when_found()
    {
        var id = Guid.NewGuid();
        var dto = new UserDto(id, "Jane Smith", "jsmith@adticorp.com", true, DateTimeOffset.UtcNow);
        _factory.QueryDispatcher
            .SendAsync<GetUserByIdQuery, UserDto?>(Arg.Any<GetUserByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserDto?>>(Result.Success<UserDto?>(dto)));

        var response = await _client.GetAsync($"api/intranet/users/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UserDto>();
        body!.Id.Should().Be(id);
        body.DisplayName.Should().Be("Jane Smith");
        body.Email.Should().Be("jsmith@adticorp.com");
    }

    [Fact]
    public async Task GetById_returns_404_when_user_not_found()
    {
        _factory.QueryDispatcher
            .SendAsync<GetUserByIdQuery, UserDto?>(Arg.Any<GetUserByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserDto?>>(
                Result.Failure<UserDto?>(new AppError("User.NotFound", "Not found."))));

        var response = await _client.GetAsync($"api/intranet/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT api/intranet/users/by-email ──────────────────────────────────────

    [Fact]
    public async Task UpdateByEmail_returns_200_with_updated_user()
    {
        var id = Guid.NewGuid();
        var dto = new UserDto(id, "Jane Updated", "jsmith@adticorp.com", true, DateTimeOffset.UtcNow);
        _factory.CommandDispatcher
            .SendAsync(Arg.Any<UpdateUserByEmailCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserDto>>(Result.Success(dto)));

        var request = new { Email = "jsmith@adticorp.com", NewDisplayName = "Jane Updated" };
        var response = await _client.PutAsJsonAsync("api/intranet/users/by-email", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UserDto>();
        body!.DisplayName.Should().Be("Jane Updated");
    }

    [Fact]
    public async Task UpdateByEmail_returns_404_when_user_not_found()
    {
        _factory.CommandDispatcher
            .SendAsync(Arg.Any<UpdateUserByEmailCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserDto>>(
                Result.Failure<UserDto>(new AppError("User.NotFound", "Not found."))));

        var request = new { Email = "ghost@adticorp.com", NewDisplayName = "Ghost" };
        var response = await _client.PutAsJsonAsync("api/intranet/users/by-email", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateByEmail_returns_400_on_bad_input()
    {
        _factory.CommandDispatcher
            .SendAsync(Arg.Any<UpdateUserByEmailCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<UserDto>>(
                Result.Failure<UserDto>(new AppError("User.InvalidInput", "Email is invalid."))));

        var request = new { Email = "", NewDisplayName = "" };
        var response = await _client.PutAsJsonAsync("api/intranet/users/by-email", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
