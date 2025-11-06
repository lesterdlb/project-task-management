using FluentValidation;
using Microsoft.AspNetCore.Identity;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.Extensions;
using ProjectManagement.Api.Common.Services.Auth;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Auth;

internal sealed class Login : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost(
                "api/auth/login",
                async (
                    LoginDto dto,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendCommandAsync<LoginCommand, Result<LoginResponse>>(
                        new LoginCommand(dto),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : result.ToProblemDetails();
                }
            )
            .WithTags(nameof(Auth))
            .AllowAnonymous();
    }

    internal sealed record LoginCommand(LoginDto Dto) : ICommand<Result<LoginResponse>>;

    internal sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(c => c.Dto.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(c => c.Dto.Password)
                .NotEmpty();
        }
    }

    internal sealed class LoginCommandHandler(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ITokenService tokenService)
        : ICommandHandler<LoginCommand, Result<LoginResponse>>
    {
        public async Task<Result<LoginResponse>> HandleAsync(LoginCommand command,
            CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByEmailAsync(command.Dto.Email);

            if (user is null)
            {
                return Result.Failure<LoginResponse>(Error.Auth.InvalidCredentials);
            }

            if (!await userManager.IsEmailConfirmedAsync(user))
            {
                return Result.Failure<LoginResponse>(Error.Auth.EmailNotConfirmed);
            }

            var result = await signInManager.CheckPasswordSignInAsync(
                user,
                command.Dto.Password,
                lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                return Result.Failure<LoginResponse>(Error.Auth.InvalidCredentials);
            }

            var roles = await userManager.GetRolesAsync(user);
            var token = await tokenService.GenerateToken(user, roles);

            return new LoginResponse(token, user.Email!, user.FullName);
        }
    }

    public sealed class LoginDto
    {
        public required string Email { get; init; }
        public required string Password { get; init; }
    }

    public sealed record LoginResponse(string Token, string Email, string FullName);
}
