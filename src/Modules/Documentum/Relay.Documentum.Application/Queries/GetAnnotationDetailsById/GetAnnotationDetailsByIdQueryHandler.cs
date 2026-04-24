using Relay.Documentum.Contracts.Dtos;
using Relay.Documentum.Domain.Repositories;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetAnnotationDetailsById;

internal sealed class GetAnnotationDetailsByIdQueryHandler : IQueryHandler<GetAnnotationDetailsByIdQuery, AnnotationDetailsDto>
{
    private readonly IAnnotationRepository _annotations;

    public GetAnnotationDetailsByIdQueryHandler(IAnnotationRepository annotations)
    {
        _annotations = annotations ?? throw new ArgumentNullException(nameof(annotations));
    }

    public async Task<Result<AnnotationDetailsDto>> HandleAsync(GetAnnotationDetailsByIdQuery query, CancellationToken cancellationToken = default)
    {
        var annotation = await _annotations.GetByIdAsync(query.AnnotationId, cancellationToken);
        if (annotation is null)
            return Result.Failure<AnnotationDetailsDto>(new AppError("Annotation.NotFound", $"Annotation '{query.AnnotationId}' was not found."));

        return Result.Success(new AnnotationDetailsDto(annotation.Path, annotation.CreatedBy));
    }
}
