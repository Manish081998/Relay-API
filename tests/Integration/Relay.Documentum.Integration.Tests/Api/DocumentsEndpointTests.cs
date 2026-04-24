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
using Relay.Documentum.Application.Commands.UpdateDocumentById;
using Relay.Documentum.Application.Queries.GetDocumentById;
using Relay.Documentum.Application.Queries.GetDocumentByName;
using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Integration.Tests.Common;
using Relay.SharedKernel.Application;
using Xunit;

namespace Relay.Documentum.Integration.Tests.Api;

[Collection(DocumentumApiCollection.Name)]
public sealed class DocumentsEndpointTests
{
    private readonly DocumentumApiFactory _factory;
    private readonly HttpClient _client;

    public DocumentsEndpointTests(DocumentumApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        factory.QueryDispatcher.ClearSubstitute();
        factory.CommandDispatcher.ClearSubstitute();
    }

    // ── GET api/documentum/documents/{id} ────────────────────────────────────

    [Fact]
    public async Task GetById_returns_200_with_document_when_found()
    {
        var id = Guid.NewGuid();
        var dto = new DocumentDto(id, "Annual Report", "Active", Guid.NewGuid(), 2048, DateTimeOffset.UtcNow, null);
        _factory.QueryDispatcher
            .SendAsync<GetDocumentByIdQuery, DocumentDto?>(Arg.Any<GetDocumentByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DocumentDto?>>(Result.Success<DocumentDto?>(dto)));

        var response = await _client.GetAsync($"api/documentum/documents/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<DocumentDto>();
        body!.Id.Should().Be(id);
        body.Title.Should().Be("Annual Report");
    }

    [Fact]
    public async Task GetById_returns_404_when_document_not_found()
    {
        _factory.QueryDispatcher
            .SendAsync<GetDocumentByIdQuery, DocumentDto?>(Arg.Any<GetDocumentByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DocumentDto?>>(
                Result.Failure<DocumentDto?>(new AppError("Document.NotFound", "Not found."))));

        var response = await _client.GetAsync($"api/documentum/documents/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET api/documentum/documents/search?name=xxx ─────────────────────────

    [Fact]
    public async Task Search_returns_200_with_document_list()
    {
        var dto = new DocumentDto(Guid.NewGuid(), "Policy Doc", "Active", Guid.NewGuid(), 512, DateTimeOffset.UtcNow, null);
        IReadOnlyList<DocumentDto> list = [dto];
        _factory.QueryDispatcher
            .SendAsync<GetDocumentByNameQuery, IReadOnlyList<DocumentDto>>(Arg.Any<GetDocumentByNameQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<DocumentDto>>>(Result.Success(list)));

        var response = await _client.GetAsync("api/documentum/documents/search?name=Policy");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<DocumentDto>>();
        body!.Should().HaveCount(1);
        body[0].Title.Should().Be("Policy Doc");
    }

    [Fact]
    public async Task Search_returns_400_when_query_fails()
    {
        _factory.QueryDispatcher
            .SendAsync<GetDocumentByNameQuery, IReadOnlyList<DocumentDto>>(Arg.Any<GetDocumentByNameQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<IReadOnlyList<DocumentDto>>>(
                Result.Failure<IReadOnlyList<DocumentDto>>(new AppError("Document.NameRequired", "Name is required."))));

        var response = await _client.GetAsync("api/documentum/documents/search?name=");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── PUT api/documentum/documents/{id} ─────────────────────────────────────

    [Fact]
    public async Task UpdateById_returns_200_with_updated_document()
    {
        var id = Guid.NewGuid();
        var dto = new DocumentDto(id, "Updated Title", "Active", Guid.NewGuid(), 4096, DateTimeOffset.UtcNow, null);
        _factory.CommandDispatcher
            .SendAsync(Arg.Any<UpdateDocumentByIdCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DocumentDto>>(Result.Success(dto)));

        var request = new { Title = "Updated Title", StoragePath = "/docs/report.pdf", SizeInBytes = 4096 };
        var response = await _client.PutAsJsonAsync($"api/documentum/documents/{id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<DocumentDto>();
        body!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task UpdateById_returns_404_when_document_not_found()
    {
        _factory.CommandDispatcher
            .SendAsync(Arg.Any<UpdateDocumentByIdCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DocumentDto>>(
                Result.Failure<DocumentDto>(new AppError("Document.NotFound", "Not found."))));

        var request = new { Title = "Title", StoragePath = "/docs/file.pdf", SizeInBytes = 1024 };
        var response = await _client.PutAsJsonAsync($"api/documentum/documents/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateById_returns_400_on_bad_input()
    {
        _factory.CommandDispatcher
            .SendAsync(Arg.Any<UpdateDocumentByIdCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<DocumentDto>>(
                Result.Failure<DocumentDto>(new AppError("Document.InvalidInput", "Invalid input."))));

        var request = new { Title = "", StoragePath = "", SizeInBytes = -1 };
        var response = await _client.PutAsJsonAsync($"api/documentum/documents/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
