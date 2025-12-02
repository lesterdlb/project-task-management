using System.Linq.Expressions;
using ProjectManagement.Api.Core.Domain.Entities;
using ProjectManagement.Api.Core.Application.DTOs.User;
using ProjectManagement.Api.Core.Application.Models;
using ProjectManagement.Api.Core.Application.Services.Sorting;
using ProjectManagement.Api.Features.Users;

namespace ProjectManagement.Api.Core.Application.Mappings;

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

    extension(User user)
    {
        public TDto ToUserDto<TDto>(List<LinkDto>? links = null) where TDto : UserDto, new()
        {
            return new TDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FullName = user.FullName,
                Links = links ?? []
            };
        }

        public void UpdateFromDto(UpdateUser.UpdateUserDto dto)
        {
            user.UserName = dto.UserName;
            user.Email = dto.Email;
            user.FullName = dto.FullName;
        }
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
