using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProjectManagement.Api.Core.Application.Authorization;
using ProjectManagement.Api.Core.Domain.Abstractions;
using ProjectManagement.Api.Core.Domain.Entities;
using ProjectManagement.Api.Core.Domain.Enums;
using ProjectManagement.Api.Core.Application.DTOs.User;
using ProjectManagement.Api.Infrastructure.Extensions;
using ProjectManagement.Api.Core.Application.Mappings;
using ProjectManagement.Api.Core.Application.Services.Auth;
using ProjectManagement.Api.Infrastructure.Slices;
using ProjectManagement.Api.Core.Application.Validation.Entities;
using ProjectManagement.Api.Constants;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Users;

internal sealed class UpdateUser : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut(
                EndpointNames.Users.Routes.ById,
                async (
                    Guid id,
                    UpdateUserDto updateUserDto,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendCommandAsync<UpdateUserCommand, Result>(
                        new UpdateUserCommand(id, updateUserDto),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.NoContent()
                        : result.ToProblemDetails();
                }
            )
            .WithName(EndpointNames.Users.Names.UpdateUser)
            .WithTags(EndpointNames.Users.GroupName)
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

    public sealed record UpdateUserDto(
        string UserName,
        string Email,
        string FullName
    );
}
