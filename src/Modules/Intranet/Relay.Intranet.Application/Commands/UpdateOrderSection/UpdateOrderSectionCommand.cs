using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Commands.UpdateOrderSection;

public sealed record UpdateOrderSectionCommand(
    string OrderGuid,
    string RepPo,
    string UserId,
    string SectionName,
    string FileName,
    string Brand,
    Dictionary<string, string> Fields) : ICommand<bool>;
