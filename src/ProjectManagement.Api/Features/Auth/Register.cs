using FluentValidation;
using Microsoft.AspNetCore.Identity;
using ProjectManagement.Api.Core.Domain.Abstractions;
using ProjectManagement.Api.Core.Domain.Entities;
using ProjectManagement.Api.Core.Domain.Enums;
using ProjectManagement.Api.Infrastructure.Extensions;
using ProjectManagement.Api.Core.Application.Services.Email;
using ProjectManagement.Api.Infrastructure.Slices;
using ProjectManagement.Api.Constants;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Auth;

internal sealed class Register : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost(
                EndpointNames.Auth.Routes.Register,
                async (
                    RegisterDto registerDto,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendCommandAsync<RegisterCommand, Result>(
                        new RegisterCommand(registerDto),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.Ok(new
                        {
                            message = "Registration successful. Please check your email to confirm your account."
                        })
                        : result.ToProblemDetails();
                }
            )
            .WithName(EndpointNames.Auth.Names.Register)
            .WithTags(EndpointNames.Auth.GroupName)
            .AllowAnonymous();
    }

    internal sealed record RegisterCommand(RegisterDto Dto) : ICommand<Result>;

    internal sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(c => c.Dto.UserName)
                .NotEmpty()
                .Length(3, 50)
                .Matches("^[a-zA-Z0-9_-]+$")
                .WithMessage("Username can only contain letters, numbers, underscores, and hyphens");

            RuleFor(c => c.Dto.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(c => c.Dto.FullName)
                .NotEmpty()
                .Length(2, 100);

            RuleFor(c => c.Dto.Password)
                .NotEmpty()
                .MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

            RuleFor(c => c.Dto.ConfirmPassword)
                .Equal(c => c.Dto.Password)
                .WithMessage("Passwords do not match");
        }
    }

    internal sealed class RegisterCommandHandler(
        UserManager<User> userManager,
        IEmailService emailService,
        ILogger<RegisterCommandHandler> logger)
        : ICommandHandler<RegisterCommand, Result>
    {
        public async Task<Result> HandleAsync(RegisterCommand command, CancellationToken cancellationToken = default)
        {
            var user = new User
            {
                UserName = command.Dto.UserName,
                Email = command.Dto.Email,
                FullName = command.Dto.FullName,
                Role = UserRole.Member,
                CreatedBy = Guid.Empty,
                CreatedAtUtc = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, command.Dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure(AuthErrors.RegistrationFailed(errors));
            }

            await userManager.AddToRoleAsync(user, nameof(UserRole.Member));

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink =
                $"https://yourdomain.com/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            try
            {
                await emailService.SendEmailConfirmationAsync(user.Email, user.UserName, confirmationLink);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send confirmation email to {Email}", user.Email);
            }

            return Result.Success();
        }
    }

    public sealed record RegisterDto(
        string UserName,
        string Email,
        string FullName,
        string Password,
        string ConfirmPassword);
}
