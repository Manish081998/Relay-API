using Microsoft.Extensions.DependencyInjection;
using Relay.SharedKernel.Application;

namespace Relay.CrossCutting.Dispatching;

public sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<Result> SendAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        var handler = (ICommandHandlerBase)_serviceProvider.GetRequiredService(handlerType);
        return handler.HandleAsync(command, cancellationToken);
    }

    public Task<Result<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        var handler = (ICommandHandlerBase<TResponse>)_serviceProvider.GetRequiredService(handlerType);
        return handler.HandleAsync(command, cancellationToken);
    }
}
