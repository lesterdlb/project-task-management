using System.Linq.Expressions;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.Domain.Enums;
using ProjectManagement.Api.Common.DTOs.User;
using ProjectManagement.Api.Common.Services.Sorting;
using ProjectManagement.Api.Features.Users;

namespace ProjectManagement.Api.Common.Mappings;

internal static class UserMappings
{
    public static Expression<Func<User, TDto>> ProjectToUserDto<TDto>() where TDto : UserDto, new()
    {
        return u => new TDto
        {
            Id = u.Id,
            UserName = u.UserName!,
            Email = u.Email!,
            FullName = u.FullName
        };
    }

    public static TDto ToUserDto<TDto>(this User user) where TDto : UserDto, new()
    {
        return new TDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            FullName = user.FullName
        };
    }

    public static void UpdateFromDto(this User user, UpdateUser.UpdateUserDto dto)
    {
        user.UserName = dto.UserName;
        user.Email = dto.Email;
        user.FullName = dto.FullName;
    }

    public static readonly SortMappingDefinition<UserDto, User> UserSortMapping = new()
    {
        Mappings =
        [
            new SortMapping(nameof(UserDto.UserName), nameof(User.UserName)),
            new SortMapping(nameof(UserDto.Email), nameof(User.Email)),
            new SortMapping(nameof(UserDto.FullName), nameof(User.FullName))
        ]
    };
}
