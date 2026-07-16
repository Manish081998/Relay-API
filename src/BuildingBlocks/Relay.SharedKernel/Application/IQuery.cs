namespace Relay.SharedKernel.Application;

public interface IQuery<TResponse> { }

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}

/// <summary>
/// Single entry point for all queries. Controllers inject this instead of individual handlers.
/// Both type parameters are required so the handler is resolved statically — enabling F12 navigation.
/// </summary>
public interface IQueryDispatcher
{
    Task<Result<TResponse>> SendAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>;
}
