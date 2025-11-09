using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProjectManagement.Api.Common.Authorization;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.Domain.Enums;
using ProjectManagement.Api.Common.DTOs.User;
using ProjectManagement.Api.Common.Extensions;
using ProjectManagement.Api.Common.Mappings;
using ProjectManagement.Api.Common.Services.Auth;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Common.Validators;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Users;

internal sealed class UpdateUser : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut(
                "api/users/{userId:guid}",
                async (
                    Guid userId,
                    UpdateUserDto updateUserDto,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendCommandAsync<UpdateUserCommand, Result>(
                        new UpdateUserCommand(userId, updateUserDto),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.NoContent()
                        : result.ToProblemDetails();
                }
            )
            .WithTags(nameof(Users))
            .RequirePermissions(Permissions.Users.Write);
    }

    internal sealed record UpdateUserCommand(Guid Id, UpdateUserDto Dto) : ICommand<Result>;

    internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(c => c.Dto.UserName).ValidateUserName();
            RuleFor(c => c.Dto.Email).ValidateEmail();
            RuleFor(c => c.Dto.FullName).ValidateFullName();
        }
    }

    internal sealed class UpdateUserCommandHandler(
        UserManager<User> userManager,
        ICurrentUserService currentUserService)
        : ICommandHandler<UpdateUserCommand, Result>
    {
        public async Task<Result> HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken = default)
        {
            var currentUserId = currentUserService.UserId;
            if (currentUserId is null)
            {
                return Result.Failure(Error.Unauthorized);
            }

            var user = await userManager.FindByIdAsync(command.Id.ToString());

            if (user is null)
            {
                return Result.Failure(Error.NotFound);
            }

            if (user.Id != currentUserId && !currentUserService.IsInRole(nameof(UserRole.Admin)))
            {
                return Result.Failure(Error.Forbidden);
            }

            user.UpdateFromDto(command.Dto);

            try
            {
                await userManager.UpdateAsync(user);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
            {
                if (pgEx.ConstraintName?.Contains("email") is true ||
                    pgEx.ConstraintName?.Contains("user_name") is true)
                {
                    return Result.Failure<UserDto>(Error.Conflict);
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

    public sealed class UpdateUserDto
    {
        public string UserName { get; init; }
        public string Email { get; init; }
        public string FullName { get; init; }
    }
}
