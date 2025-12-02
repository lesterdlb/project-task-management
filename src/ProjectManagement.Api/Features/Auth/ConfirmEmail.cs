using FluentValidation;
using Microsoft.AspNetCore.Identity;
using ProjectManagement.Api.Core.Domain.Abstractions;
using ProjectManagement.Api.Core.Domain.Entities;
using ProjectManagement.Api.Infrastructure.Extensions;
using ProjectManagement.Api.Infrastructure.Slices;
using ProjectManagement.Api.Constants;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Auth;

internal sealed class ConfirmEmail : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost(
                EndpointNames.Auth.Routes.ConfirmEmail,
                async (ConfirmEmailDto dto, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendCommandAsync<ConfirmEmailCommand, Result>(
                        new ConfirmEmailCommand(dto),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.Ok(new { message = "Email confirmed successfully. You can now log in." })
                        : result.ToProblemDetails();
                }
            )
            .WithName(EndpointNames.Auth.Names.ConfirmEmail)
            .WithTags(EndpointNames.Auth.GroupName)
            .AllowAnonymous();
    }

    internal sealed record ConfirmEmailCommand(ConfirmEmailDto Dto) : ICommand<Result>;

    internal sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
    {
        public ConfirmEmailCommandValidator()
        {
            RuleFor(c => c.Dto.UserId).NotEmpty();
            RuleFor(c => c.Dto.Token).NotEmpty();
        }
    }

    internal sealed class ConfirmEmailCommandHandler(UserManager<User> userManager)
        : ICommandHandler<ConfirmEmailCommand, Result>
    {
        public async Task<Result> HandleAsync(ConfirmEmailCommand command,
            CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByIdAsync(command.Dto.UserId.ToString());

            if (user is null)
            {
                return Result.Failure(Error.Auth.UserNotFound);
            }

            if (user.EmailConfirmed)
            {
                return Result.Failure(Error.Auth.EmailAlreadyConfirmed);
            }

            var result = await userManager.ConfirmEmailAsync(user, command.Dto.Token);

            if (result.Succeeded)
            {
                return Result.Success();
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(Error.Auth.EmailConfirmationFailed(errors));
        }
    }

    public sealed record ConfirmEmailDto(Guid UserId, string Token);
}
