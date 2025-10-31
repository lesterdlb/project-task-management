using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.DTOs.User;
using ProjectManagement.Api.Common.Extensions;
using ProjectManagement.Api.Common.Mappings;
using ProjectManagement.Api.Common.Persistence;
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
        );
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

    internal sealed class UpdateUserCommandHandler(ProjectManagementDbContext dbContext)
        : ICommandHandler<UpdateUserCommand, Result>
    {
        public async Task<Result> HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken = default)
        {
            var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Id == command.Id, cancellationToken);

            if (user is null)
            {
                return Result.Failure(Error.NotFound);
            }

            user.UpdateFromDto(command.Dto);

            try
            {
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
