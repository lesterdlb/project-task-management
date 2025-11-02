using ProjectManagement.Api.Common.Domain.Entities;

namespace ProjectManagement.Api.Common.Services.Auth;

public interface IJwtService
{
    string GenerateToken(User user, IList<string> roles);
}
