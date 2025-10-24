namespace ProjectManagement.Api.Mediator;

internal sealed class Mediator(IServiceProvider provider) : IMediator
{
    public async Task<TResult> SendCommandAsync<TCommand, TResult>(TCommand command,
        CancellationToken cancellationToken = default) where TCommand : ICommand
    {
        ArgumentNullException.ThrowIfNull(command);

        var handler = provider.GetRequiredService<ICommandHandler<TCommand, TResult>>();

        var behaviors = provider.GetServices<IPipelineBehavior<TCommand, TResult>>().ToArray();

        if (behaviors.Length == 0)
        {
            return await handler.HandleAsync(command, cancellationToken);
        }

        return await ExecutePipelineAsync(command, handler.HandleAsync, behaviors, cancellationToken);
    }

    public async Task<TResult> SendQueryAsync<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken = default) where TQuery : IQuery<TResult>
    {
        ArgumentNullException.ThrowIfNull(query);

        var handler = provider.GetRequiredService<IQueryHandler<TQuery, TResult>>();

        var behaviors = provider.GetServices<IPipelineBehavior<TQuery, TResult>>().ToArray();

        if (behaviors.Length == 0)
        {
            return await handler.HandleAsync(query, cancellationToken);
        }

        return await ExecutePipelineAsync(query, handler.HandleAsync, behaviors, cancellationToken);
    }

    public async Task PublishAsync<TNotification>(TNotification notification,
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        var handlers = provider.GetServices<INotificationHandler<TNotification>>();

        var tasks = handlers.Select(handler => handler.HandleAsync(notification, cancellationToken));

        await Task.WhenAll(tasks);
    }

    private static Task<TResult> ExecutePipelineAsync<TRequest, TResult>(
        TRequest request,
        Func<TRequest, CancellationToken, Task<TResult>> handler,
        IPipelineBehavior<TRequest, TResult>[] behaviors,
        CancellationToken cancellationToken)
    {
        return ExecuteNextBehavior(0);

        Task<TResult> ExecuteNextBehavior(int index)
        {
            if (index < behaviors.Length)
            {
                return behaviors[index].HandleAsync(
                    request,
                    () => ExecuteNextBehavior(index + 1),
                    cancellationToken);
            }

            return handler(request, cancellationToken);
        }
    }
}
