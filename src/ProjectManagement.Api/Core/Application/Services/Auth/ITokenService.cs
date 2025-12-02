using ProjectManagement.Api.Core.Domain.Entities;

namespace ProjectManagement.Api.Core.Application.Services.Auth;

public interface ITokenService
{
    Task<string> GenerateToken(User user, IList<string> roles);
}
