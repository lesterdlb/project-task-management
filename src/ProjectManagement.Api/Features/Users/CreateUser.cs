using FluentValidation;
using Microsoft.AspNetCore.Identity;
using ProjectManagement.Api.Core.Application.Authorization;
using ProjectManagement.Api.Core.Domain.Abstractions;
using ProjectManagement.Api.Core.Domain.Entities;
using ProjectManagement.Api.Core.Domain.Enums;
using ProjectManagement.Api.Core.Application.DTOs.User;
using ProjectManagement.Api.Infrastructure.Extensions;
using ProjectManagement.Api.Core.Application.Mappings;
using ProjectManagement.Api.Core.Application.Services.Auth;
using ProjectManagement.Api.Core.Application.Services.Links;
using ProjectManagement.Api.Infrastructure.Slices;
using ProjectManagement.Api.Core.Application.Validation.Entities;
using ProjectManagement.Api.Constants;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Users;

internal sealed class CreateUser : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost(
                EndpointNames.Users.Routes.Base,
                async (
                    CreateUserDto createUserDto,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendCommandAsync<CreateUserCommand, Result<CreateUserResponse>>(
                        new CreateUserCommand(createUserDto),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.Created(result.Value.Location, result.Value.UserDto)
                        : result.ToProblemDetails();
                }
            )
            .WithName(EndpointNames.Users.Names.CreateUser)
            .WithTags(EndpointNames.Users.GroupName)
            .RequirePermissions(Permissions.Users.Write);
    }

    internal sealed record CreateUserCommand(CreateUserDto Dto) : ICommand<Result<CreateUserResponse>>;

    internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(c => c.Dto.UserName).ValidateUserName();
            RuleFor(c => c.Dto.Email).ValidateEmail();
            RuleFor(c => c.Dto.FullName).ValidateFullName();
        }
    }

    internal sealed class CreateUserCommandHandler(
        ICurrentUserService currentUserService,
        UserManager<User> userManager,
        ILinkService linkService
    )
        : ICommandHandler<CreateUserCommand, Result<CreateUserResponse>>
    {
        public async Task<Result<CreateUserResponse>> HandleAsync(CreateUserCommand command,
            CancellationToken cancellationToken = default)
        {
            var userId = currentUserService.UserId;
            if (userId is null)
            {
                return Result.Failure<CreateUserResponse>(Error.Unauthorized);
            }

            var user = new User
            {
                UserName = command.Dto.UserName,
                Email = command.Dto.Email,
                FullName = command.Dto.FullName,
                Role = UserRole.Member,
                EmailConfirmed = true,
                CreatedBy = userId.Value,
                CreatedAtUtc = DateTime.UtcNow
            };

            const string tempPassword = "P@ssW0rd1!";
            var result = await userManager.CreateAsync(user, tempPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Failure<CreateUserResponse>(UserErrors.CreateFailed(errors));
            }

            var roleResult = await userManager.AddToRoleAsync(user, user.Role.ToString());
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                return Result.Failure<CreateUserResponse>(UserErrors.CreateFailed(errors));
            }

            var userDto = user.ToUserDto<UserDto>(
                linkService.CreateLinksForItem(
                    EndpointNames.Users.Names.CreateUser,
                    EndpointNames.Users.Names.UpdateUser,
                    EndpointNames.Users.Names.DeleteUser,
                    user.Id)
            );

            return new CreateUserResponse(
                userDto,
                linkService.CreateHref(EndpointNames.Projects.Names.GetProject, new { id = user.Id })
            );
        }
    }

    public sealed record CreateUserDto(
        string UserName,
        string Email,
        string FullName
    );

    public sealed record CreateUserResponse(UserDto UserDto, string Location);
}
