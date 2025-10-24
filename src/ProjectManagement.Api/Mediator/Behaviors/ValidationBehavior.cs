using FluentValidation;

namespace ProjectManagement.Api.Mediator.Behaviors;

internal sealed class ValidationBehavior<TRequest, TResponse>(IServiceProvider serviceProvider)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> HandleAsync(TRequest input, Func<Task<TResponse>> next,
        CancellationToken cancellationToken = default)
    {
        var services = serviceProvider.GetServices<IValidator<TRequest>>();
        var validators = services as IValidator<TRequest>[] ?? [.. services];
        if (!validators.Any())
        {
            return await next();
        }

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(input, cancellationToken)));

        var validationResult = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (validationResult.Count != 0)
        {
            throw new ValidationException(validationResult);
        }

        return await next();
    }
}
