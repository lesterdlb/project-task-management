using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProjectManagement.Api.Common.Authorization;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.DTOs.User;
using ProjectManagement.Api.Common.Extensions;
using ProjectManagement.Api.Common.Mappings;
using ProjectManagement.Api.Common.Persistence;
using ProjectManagement.Api.Common.Slices;
using ProjectManagement.Api.Common.Validators;
using ProjectManagement.Api.Mediator;

namespace ProjectManagement.Api.Features.Users;

internal sealed class CreateUser : ISlice
{
    public void AddEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost(
                "api/users",
                async (
                    CreateUserDto createUserDto,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.SendCommandAsync<CreateUserCommand, Result<UserDto>>(
                        new CreateUserCommand(createUserDto),
                        cancellationToken);

                    return result.IsSuccess
                        ? Results.Created($"api/users/{result.Value.Id}", result.Value)
                        : result.ToProblemDetails();
                }
            )
            .WithTags(nameof(Users))
            .RequirePermissions(Permissions.Users.Write);
    }

    internal sealed record CreateUserCommand(CreateUserDto Dto) : ICommand<Result<UserDto>>;

    internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(c => c.Dto.UserName).ValidateUserName();
            RuleFor(c => c.Dto.Email).ValidateEmail();
            RuleFor(c => c.Dto.FullName).ValidateFullName();
        }
    }

    internal sealed class CreateUserCommandHandler(ProjectManagementDbContext dbContext)
        : ICommandHandler<CreateUserCommand, Result<UserDto>>
    {
        public async Task<Result<UserDto>> HandleAsync(CreateUserCommand command,
            CancellationToken cancellationToken = default)
        {
            var user = command.Dto.ToEntity();

            try
            {
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync(cancellationToken);
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

            return user.ToUserDto<UserDto>();
        }
    }

    public sealed class CreateUserDto
    {
        public string UserName { get; init; }
        public string Email { get; init; }
        public string FullName { get; init; }
    }
}
