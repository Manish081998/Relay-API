using Relay.SharedKernel.Application;

namespace Relay.Intranet.Application.Commands.UpdatePlantCode;

public sealed record UpdatePlantCodeCommand(
    string OrderGuid,
    string Po,
    string UserId,
    string LineNumber,
    string NewPlantCode,
    bool IsSecondaryPlant) : ICommand<bool>;
