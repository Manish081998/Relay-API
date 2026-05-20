namespace Relay.SharedKernel.Application;

public interface ICommand { }

public interface ICommand<TResponse> { }

public interface ICommandHandlerBase
{
    Task<Result> HandleAsync(ICommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandlerBase<TResponse>
{
    Task<Result<TResponse>> HandleAsync(ICommand<TResponse> command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand> : ICommandHandlerBase
    where TCommand : ICommand
{
    Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    Task<Result> ICommandHandlerBase.HandleAsync(ICommand command, CancellationToken cancellationToken)
        => HandleAsync((TCommand)command, cancellationToken);
}

public interface ICommandHandler<in TCommand, TResponse> : ICommandHandlerBase<TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    Task<Result<TResponse>> ICommandHandlerBase<TResponse>.HandleAsync(ICommand<TResponse> command, CancellationToken cancellationToken)
        => HandleAsync((TCommand)command, cancellationToken);
}

/// <summary>
/// Single entry point for all commands. Controllers inject this instead of individual handlers.
/// </summary>
public interface ICommandDispatcher
{
    Task<Result> SendAsync(ICommand command, CancellationToken cancellationToken = default);
    Task<Result<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
}
