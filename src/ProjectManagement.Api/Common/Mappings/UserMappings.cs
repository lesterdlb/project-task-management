using System.Linq.Expressions;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.Domain.Enums;
using ProjectManagement.Api.Common.DTOs.User;
using ProjectManagement.Api.Common.Services.Sorting;
using ProjectManagement.Api.Features.Users;

namespace ProjectManagement.Api.Common.Mappings;

internal static class UserMappings
{
    public static Expression<Func<User, TDto>> ProjectToUserDto<TDto>() where TDto : IUserDto, new()
    {
        return u => new TDto
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            FullName = u.FullName
        };
    }

    public static TDto ToUserDto<TDto>(this User user) where TDto : IUserDto, new()
    {
        return new TDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName
        };
    }

    public static User ToEntity(this CreateUser.CreateUserDto user)
    {
        return new User
        {
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            Role = UserRole.Member // Temporarily set to member
        };
    }

    public static readonly SortMappingDefinition<IUserDto, User> UserSortMapping = new()
    {
        Mappings =
        [
            new SortMapping(nameof(IUserDto.UserName), nameof(User.UserName)),
            new SortMapping(nameof(IUserDto.Email), nameof(User.Email)),
            new SortMapping(nameof(IUserDto.FullName), nameof(User.FullName))
        ]
    };
}
