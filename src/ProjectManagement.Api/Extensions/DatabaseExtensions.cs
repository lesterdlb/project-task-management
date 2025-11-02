using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            if (!await roleManager.RoleExistsAsync(role.ToString()))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role.ToString()));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<User> userManager, ProjectManagementDbContext dbContext)
    {
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

        var memberUser = new User
        {
            UserName = "member",
            Email = "member@projectmanagement.com",
            FullName = "John Doe",
            Role = UserRole.Member,
            EmailConfirmed = true,
            CreatedBy = Guid.Empty,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, "P@ssW0rd1!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, nameof(UserRole.Admin));
        }

        result = await userManager.CreateAsync(memberUser, "P@ssW0rd1!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(memberUser, nameof(UserRole.Member));
        }
    }
}
