using FluentValidation;
using Microsoft.AspNetCore.Identity;
using ProjectManagement.Api.Common.Authorization;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.Domain.Enums;
using ProjectManagement.Api.Common.DTOs.User;
using ProjectManagement.Api.Common.Extensions;
using ProjectManagement.Api.Common.Mappings;
using ProjectManagement.Api.Common.Services.Auth;
using ProjectManagement.Api.Common.Services.Links;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Common.Validators;
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
                return Result.Failure<CreateUserResponse>(Error.User.CreateFailed(errors));
            }

            var roleResult = await userManager.AddToRoleAsync(user, user.Role.ToString());
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                return Result.Failure<CreateUserResponse>(Error.User.CreateFailed(errors));
            }

            var userDto = user.ToUserDto<UserDto>();

            userDto.Links = linkService.CreateLinksForItem(
                EndpointNames.Users.Names.CreateUser,
                EndpointNames.Users.Names.UpdateUser,
                EndpointNames.Users.Names.DeleteUser,
                userDto.Id);

            return new CreateUserResponse
            {
                UserDto = userDto,
                Location = linkService.CreateHref(
                    EndpointNames.Projects.Names.GetProject,
                    new { id = user.Id })
            };
        }
    }

    public sealed class CreateUserDto
    {
        public string UserName { get; init; }
        public string Email { get; init; }
        public string FullName { get; init; }
    }

    public sealed class CreateUserResponse
    {
        public UserDto UserDto { get; init; }
        public string Location { get; init; }
    }
}
