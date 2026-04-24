using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Relay.Documentum.Application.Queries.GetAnnotationDetailsById;
using Relay.Documentum.Application.Tests.Builders;
using Relay.Documentum.Domain.Repositories;
using Xunit;

namespace Relay.Documentum.Application.Tests.Queries;

public sealed class GetAnnotationDetailsByIdQueryHandlerTests
{
    private readonly IAnnotationRepository _repo = Substitute.For<IAnnotationRepository>();
    private readonly GetAnnotationDetailsByIdQueryHandler _handler;

    public GetAnnotationDetailsByIdQueryHandlerTests()
    {
        _handler = new GetAnnotationDetailsByIdQueryHandler(_repo);
    }

    [Fact]
    public async Task HandleAsync_returns_dto_when_annotation_exists()
    {
        var annotation = AnnotationBuilder.Build(id: 42, path: "/store/doc.pdf", createdBy: "admin@adticorp.com");
        _repo.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns(annotation);

        var result = await _handler.HandleAsync(new GetAnnotationDetailsByIdQuery(42));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Path.Should().Be("/store/doc.pdf");
        result.Value.CreatedBy.Should().Be("admin@adticorp.com");
    }

    [Fact]
    public async Task HandleAsync_returns_failure_when_annotation_not_found()
    {
        _repo.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((Relay.Documentum.Domain.Aggregates.Annotation?)null);

        var result = await _handler.HandleAsync(new GetAnnotationDetailsByIdQuery(999));

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Annotation.NotFound");
    }

    [Fact]
    public async Task HandleAsync_calls_repository_with_correct_id()
    {
        _repo.GetByIdAsync(7, Arg.Any<CancellationToken>())
            .Returns((Relay.Documentum.Domain.Aggregates.Annotation?)null);

        await _handler.HandleAsync(new GetAnnotationDetailsByIdQuery(7));

        await _repo.Received(1).GetByIdAsync(7, Arg.Any<CancellationToken>());
    }
}
