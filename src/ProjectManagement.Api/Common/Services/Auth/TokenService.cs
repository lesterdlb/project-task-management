using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProjectManagement.Api.Common.Authorization;
using ProjectManagement.Api.Common.Domain.Entities;

namespace ProjectManagement.Api.Common.Services.Auth;

public class TokenService(IOptions<TokenOptions> jwtOptions, RoleManager<IdentityRole<Guid>> roleManager)
    : ITokenService
{
    private readonly TokenOptions _tokenOptions = jwtOptions.Value;

    public async Task<string> GenerateToken(User user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("username", user.UserName!)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var permissions = (await Task.WhenAll(
                roles.Select(async roleName =>
                {
                    var role = await roleManager.FindByNameAsync(roleName);
                    if (role is null)
                    {
                        return [];
                    }

                    var roleClaims = await roleManager.GetClaimsAsync(role);
                    return roleClaims.Where(c => c.Type == Permissions.ClaimType);
                })))
            .SelectMany(roleClaims => roleClaims)
            .Select(roleClaim => roleClaim.Value)
            .ToHashSet();

        claims.AddRange(permissions.Select(permission => new Claim(Permissions.ClaimType, permission)));

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenOptions.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _tokenOptions.Issuer,
            audience: _tokenOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_tokenOptions.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public sealed class TokenOptions
{
    public required string SecretKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int ExpirationMinutes { get; init; } = 30;
}
