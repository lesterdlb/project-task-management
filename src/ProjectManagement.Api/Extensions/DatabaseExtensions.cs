using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Authorization;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.Domain.Enums;
using ProjectManagement.Api.Common.Persistence;

namespace ProjectManagement.Api.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<ProjectManagementDbContext>();

        try
        {
            await dbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Application database migrations applied.");
        }
        catch (Exception e)
        {
            app.Logger.LogError(e, "An error occurred while migrating the database.");
            throw;
        }
    }

    public static async Task SeedInitialDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProjectManagementDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        try
        {
            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager, dbContext);
        }
        catch (Exception e)
        {
            app.Logger.LogError(e, "An error occurred while seeding initial data.");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var role in Enum.GetValues<UserRole>())
        {
            var existingRole = await roleManager.FindByNameAsync(role.ToString());
            if (existingRole is not null)
            {
                continue;
            }

            existingRole = new IdentityRole<Guid>(role.ToString());
            await roleManager.CreateAsync(existingRole);

            foreach (var permission in PermissionProvider.GetPermissionsForRole(role))
            {
                await roleManager.AddClaimAsync(existingRole, new Claim(Permissions.ClaimType, permission));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<User> userManager, ProjectManagementDbContext dbContext)
    {
        const string commonPassword = "P@ssW0rd1!";
        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        var adminUser = new User
        {
            UserName = "admin",
            Email = "admin@projectmanagement.com",
            FullName = "System Administrator",
            Role = UserRole.Admin,
            EmailConfirmed = true,
            CreatedBy = Guid.Empty,
            CreatedAtUtc = DateTime.UtcNow
        };

        var member1 = new User
        {
            UserName = "member1",
            Email = "member1@projectmanagement.com",
            FullName = "John Doe",
            Role = UserRole.Member,
            EmailConfirmed = true,
            CreatedBy = Guid.Empty,
            CreatedAtUtc = DateTime.UtcNow
        };
        var member2 = new User
        {
            UserName = "member2",
            Email = "member2@projectmanagement.com",
            FullName = "Jane Smith",
            Role = UserRole.Member,
            EmailConfirmed = true,
            CreatedBy = Guid.Empty,
            CreatedAtUtc = DateTime.UtcNow
        };
        var member3 = new User
        {
            UserName = "member3",
            Email = "member3@projectmanagement.com",
            FullName = "Bob Johnson",
            Role = UserRole.Member,
            EmailConfirmed = true,
            CreatedBy = Guid.Empty,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, commonPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, nameof(UserRole.Admin));
        }

        result = await userManager.CreateAsync(member1, commonPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(member1, nameof(UserRole.Member));
        }

        result = await userManager.CreateAsync(member2, commonPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(member2, nameof(UserRole.Member));
        }

        result = await userManager.CreateAsync(member3, commonPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(member3, nameof(UserRole.Member));
        }
    }
}
