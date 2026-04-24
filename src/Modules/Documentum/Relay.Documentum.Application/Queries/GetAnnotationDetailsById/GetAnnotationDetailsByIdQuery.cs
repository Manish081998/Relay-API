using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetAnnotationDetailsById;

public sealed record GetAnnotationDetailsByIdQuery(int AnnotationId) : IQuery<AnnotationDetailsDto>;
