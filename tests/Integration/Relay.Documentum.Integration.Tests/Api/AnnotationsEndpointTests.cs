using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Relay.Documentum.Application.Queries.GetAnnotationDetailsById;
using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Integration.Tests.Common;
using Relay.SharedKernel.Application;
using Xunit;

namespace Relay.Documentum.Integration.Tests.Api;

[Collection(DocumentumApiCollection.Name)]
public sealed class AnnotationsEndpointTests
{
    private readonly DocumentumApiFactory _factory;
    private readonly HttpClient _client;

    public AnnotationsEndpointTests(DocumentumApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        factory.QueryDispatcher.ClearSubstitute();
    }

    // ── GET api/documentum/annotations/{id} ──────────────────────────────────

    [Fact]
    public async Task GetAnnotationDetailsById_returns_200_when_found()
    {
        var dto = new AnnotationDetailsDto("/documentum/store/file.pdf", "jsmith@adticorp.com");
        _factory.QueryDispatcher
            .SendAsync<GetAnnotationDetailsByIdQuery, AnnotationDetailsDto>(Arg.Any<GetAnnotationDetailsByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AnnotationDetailsDto>>(Result.Success(dto)));

        var response = await _client.GetAsync("api/documentum/annotations/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AnnotationDetailsDto>();
        body!.Path.Should().Be("/documentum/store/file.pdf");
        body.CreatedBy.Should().Be("jsmith@adticorp.com");
    }

    [Fact]
    public async Task GetAnnotationDetailsById_returns_404_when_not_found()
    {
        _factory.QueryDispatcher
            .SendAsync<GetAnnotationDetailsByIdQuery, AnnotationDetailsDto>(Arg.Any<GetAnnotationDetailsByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AnnotationDetailsDto>>(
                Result.Failure<AnnotationDetailsDto>(new AppError("Annotation.NotFound", "Not found."))));

        var response = await _client.GetAsync("api/documentum/annotations/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
