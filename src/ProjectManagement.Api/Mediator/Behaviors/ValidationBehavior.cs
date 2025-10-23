using FluentValidation;

namespace ProjectManagement.Api.Mediator.Behaviors;

internal sealed class ValidationBehavior<TRequest, TResponse>(IServiceProvider serviceProvider)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> HandleAsync(TRequest input, Func<Task<TResponse>> next,
        CancellationToken cancellationToken = default)
    {
        var validator = serviceProvider.GetService<IValidator<TRequest>>();
        if (validator == null)
        {
            return await next();
        }

        var validationResult = await validator.ValidateAsync(input, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        return await next();
    }
}
