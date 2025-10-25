using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Domain.Exceptions;
using ProjectManagement.Api.Common.DTOs.User;
using ProjectManagement.Api.Common.Mappings;
using ProjectManagement.Api.Common.Persistence;
using ProjectManagement.Api.Common.Slices;
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
                var user = await mediator.SendCommandAsync<CreateUserCommand, UserDto>(
                    new CreateUserCommand(createUserDto),
                    cancellationToken);

                return Results.Created($"api/users/{user.Id}", user);
            }
        );
    }

    internal sealed record CreateUserCommand(CreateUserDto Dto) : ICommand<UserDto>;

    internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(c => c.Dto.UserName)
                .NotEmpty()
                .MaximumLength(50)
                .WithMessage("UserName is required and must not exceed 50 characters.");
            RuleFor(c => c.Dto.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(100)
                .WithMessage("Email is required and must be a valid email address.");
            RuleFor(c => c.Dto.FullName)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("FullName is required and must not exceed 100 characters.");
        }
    }

    internal sealed class CreateUserCommandHandler(ProjectManagementDbContext dbContext)
        : ICommandHandler<CreateUserCommand, UserDto>
    {
        public async Task<UserDto> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
        {
            var emailExists = await dbContext.Users.AnyAsync(u => u.Email == command.Dto.Email, cancellationToken);

            if (emailExists)
            {
                throw new DomainException("A user with this email already exists.");
            }

            var user = command.Dto.ToEntity();

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);

            return user.ToUserDto<UserDto>();
        }
    }

    public sealed class CreateUserDto
    {
        public string UserName { get; init; }
        public string Email { get; init; }
        public string FullName { get; init; }
    }

    internal sealed class UserDto : IUserDto
    {
        public Guid Id { get; init; }
        public string UserName { get; init; }
        public string Email { get; init; }
        public string FullName { get; init; }
    }
}
