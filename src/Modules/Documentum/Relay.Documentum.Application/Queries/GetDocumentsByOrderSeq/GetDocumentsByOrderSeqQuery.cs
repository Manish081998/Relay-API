using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Queries.GetDocumentsByOrderSeq;

public sealed record GetDocumentsByOrderSeqQuery(
    int OrderSeq, bool? IsSupportedDocument = null) : IQuery<IReadOnlyList<SalesOrderDocumentDto>>;
