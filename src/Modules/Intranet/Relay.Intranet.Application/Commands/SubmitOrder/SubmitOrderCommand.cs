using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Commands.SubmitOrder;

public sealed record SubmitOrderCommand(
    string OrderGuid,
    string Po,
    string Brand,
    string UserId) : ICommand<bool>;
