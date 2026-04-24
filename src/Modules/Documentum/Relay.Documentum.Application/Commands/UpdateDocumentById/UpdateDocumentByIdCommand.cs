using Relay.Documentum.Contracts.Dtos;
using Relay.SharedKernel.Application;

namespace Relay.Documentum.Application.Commands.UpdateDocumentById;

public sealed record UpdateDocumentByIdCommand(Guid Id,string Title,string StoragePath,long SizeInBytes) : ICommand<DocumentDto>;
