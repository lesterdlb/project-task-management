using FluentValidation;
using Microsoft.AspNetCore.Identity;
using ProjectManagement.Api.Core.Domain.Abstractions;
using ProjectManagement.Api.Core.Domain.Entities;
using ProjectManagement.Api.Infrastructure.Extensions;
using ProjectManagement.Api.Core.Application.Services.Email;
using ProjectManagement.Api.Infrastructure.Slices;
using ProjectManagement.Api.Constants;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Auth;

internal sealed class ResendConfirmation : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost(
                EndpointNames.Auth.Routes.ResendConfirmation,
                async (ResendConfirmationDto dto, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendCommandAsync<ResendConfirmationCommand, Result>(
                        new ResendConfirmationCommand(dto),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.Ok(new { message = "Confirmation email sent. Please check your inbox." })
                        : result.ToProblemDetails();
                }
            )
            .WithName(EndpointNames.Auth.Names.ResendConfirmation)
            .WithTags(EndpointNames.Auth.GroupName)
            .AllowAnonymous();
    }

    internal sealed record ResendConfirmationCommand(ResendConfirmationDto Dto) : ICommand<Result>;

    internal sealed class ResendConfirmationCommandValidator : AbstractValidator<ResendConfirmationCommand>
    {
        public ResendConfirmationCommandValidator()
        {
            RuleFor(c => c.Dto.Email).NotEmpty().EmailAddress();
        }
    }

    internal sealed class ResendConfirmationCommandHandler(
        UserManager<User> userManager,
        IEmailService emailService,
        ILogger<ResendConfirmationCommandHandler> logger)
        : ICommandHandler<ResendConfirmationCommand, Result>
    {
        public async Task<Result> HandleAsync(ResendConfirmationCommand command,
            CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByEmailAsync(command.Dto.Email);

            if (user is null)
            {
                return Result.Success();
            }

            if (user.EmailConfirmed)
            {
                return Result.Failure(AuthErrors.EmailAlreadyConfirmed);
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink =
                $"https://yourdomain.com/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            try
            {
                await emailService.SendEmailConfirmationAsync(user.Email!, user.UserName!, confirmationLink);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send confirmation email to {Email}", user.Email);
                return Result.Failure(AuthErrors.RegistrationFailed(ex.Message));
            }

            return Result.Success();
        }
    }

    public sealed record ResendConfirmationDto(string Email);
}
