using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Relay.SharedKernel.Application;
using Relay.WebTool.Application.Queries.GetSelectionById;
using Relay.WebTool.Contracts.Dtos;
using Relay.WebTool.Integration.Tests.Common;
using Xunit;

namespace Relay.WebTool.Integration.Tests.Api;

[Collection(WebToolApiCollection.Name)]
public sealed class SelectionsEndpointTests
{
    private readonly WebToolApiFactory _factory;
    private readonly HttpClient _client;

    public SelectionsEndpointTests(WebToolApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        factory.QueryDispatcher.ClearSubstitute();
    }

    // ── GET api/WebTool/selections/{id} ────────────────────────────────────

    [Fact]
    public async Task GetById_returns_200_with_selection_when_found()
    {
        var id = Guid.NewGuid();
        IReadOnlyList<SelectionOptionDto> options =
        [
            new SelectionOptionDto(Guid.NewGuid(), "Option A", "A", 1),
            new SelectionOptionDto(Guid.NewGuid(), "Option B", "B", 2)
        ];
        var dto = new SelectionDto(id, "Q4 Budget Review", Guid.NewGuid(), DateTimeOffset.UtcNow, null, options);
        _factory.QueryDispatcher
            .SendAsync<GetSelectionByIdQuery, SelectionDto?>(Arg.Any<GetSelectionByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SelectionDto?>>(Result.Success<SelectionDto?>(dto)));

        var response = await _client.GetAsync($"api/WebTool/selections/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SelectionDto>();
        body!.Id.Should().Be(id);
        body.Title.Should().Be("Q4 Budget Review");
        body.Options.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_returns_404_when_selection_not_found()
    {
        _factory.QueryDispatcher
            .SendAsync<GetSelectionByIdQuery, SelectionDto?>(Arg.Any<GetSelectionByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SelectionDto?>>(
                Result.Failure<SelectionDto?>(new AppError("Selection.NotFound", "Not found."))));

        var response = await _client.GetAsync($"api/WebTool/selections/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
