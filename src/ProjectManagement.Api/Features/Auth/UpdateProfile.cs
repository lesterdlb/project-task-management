using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.Extensions;
using ProjectManagement.Api.Common.Services.Auth;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Common.Validators;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Auth;

internal sealed class UpdateProfile : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPatch(
            "api/auth/profile",
            async (
                UpdateProfileDto updateProfileDto,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.SendCommandAsync<UpdateProfileCommand, Result>(
                    new UpdateProfileCommand(updateProfileDto),
                    cancellationToken);

                return result.IsSuccess
                    ? Results.NoContent()
                    : result.ToProblemDetails();
            }
        ).RequireAuthorization();
    }

    internal sealed record UpdateProfileCommand(UpdateProfileDto Dto) : ICommand<Result>;

    internal sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator()
        {
            RuleFor(c => c.Dto.UserName).ValidateUserName();
            RuleFor(c => c.Dto.Email).ValidateEmail();
            RuleFor(c => c.Dto.FullName).ValidateFullName();
        }
    }

    internal sealed class UpdateProfileCommandHandler(
        UserManager<User> userManager,
        ICurrentUserService currentUserService)
        : ICommandHandler<UpdateProfileCommand, Result>
    {
        public async Task<Result> HandleAsync(
            UpdateProfileCommand command,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = currentUserService.UserId;
            if (currentUserId is null)
            {
                return Result.Failure(Error.Unauthorized);
            }

            var user = await userManager.FindByIdAsync(currentUserId.Value.ToString());
            if (user is null)
            {
                return Result.Failure(Error.NotFound);
            }

            user.UserName = command.Dto.UserName;
            user.Email = command.Dto.Email;
            user.FullName = command.Dto.FullName;
            //user.UpdateFromDto(command.Dto);

            try
            {
                await userManager.UpdateAsync(user);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
            {
                if (pgEx.ConstraintName?.Contains("email") is true ||
                    pgEx.ConstraintName?.Contains("user_name") is true)
                {
                    return Result.Failure(Error.Conflict);
                }

                throw;
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Failure(Error.Conflict);
            }

            return Result.Success();
        }
    }

    public sealed class UpdateProfileDto
    {
        public required string UserName { get; init; }
        public required string Email { get; init; }
        public required string FullName { get; init; }
    }
}
