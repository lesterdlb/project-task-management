namespace ProjectManagement.Api.Mediator;

internal sealed class Mediator(IServiceProvider provider) : IMediator
{
    public async Task<TResult> SendCommandAsync<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken = default) where TCommand : ICommand<TResult>
    {
        var handler = provider.GetRequiredService<ICommandHandler<TCommand, TResult>>();

        var behaviors = provider.GetServices<IPipelineBehavior<TCommand, TResult>>().Reverse();
        var handlerDelegate = () => handler.HandleAsync(command, cancellationToken);
        foreach (var behavior in behaviors)
        {
            var next = handlerDelegate;
            handlerDelegate = () => behavior.HandleAsync(command, next, cancellationToken);
        }

        return await handlerDelegate();
    }

    public async Task<TResult> SendQueryAsync<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken = default) where TQuery : IQuery<TResult>
    {
        var handler = provider.GetRequiredService<IQueryHandler<TQuery, TResult>>();

        var behaviors = provider.GetServices<IPipelineBehavior<TQuery, TResult>>().Reverse();
        var handlerDelegate = () => handler.HandleAsync(query, cancellationToken);
        foreach (var behavior in behaviors)
        {
            var next = handlerDelegate;
            handlerDelegate = () => behavior.HandleAsync(query, next, cancellationToken);
        }

        return await handlerDelegate();
    }
}
