using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Domain.Entities;
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

        // Seed users
        try
        {
            if (!await dbContext.Users.AnyAsync())
            {
                dbContext.Users.AddRange(
                    new User
                    {
                        Id = Guid.NewGuid(),
                        UserName = "demo_user",
                        FullName = "demo user",
                        Email = "demouser@mail.com"
                    },
                    new User
                    {
                        Id = Guid.NewGuid(),
                        UserName = "another_user",
                        FullName = "another user",
                        Email = "anotheruser@mail.com"
                    },
                    new User
                    {
                        Id = Guid.NewGuid(),
                        UserName = "third_user",
                        FullName = "third user",
                        Email = "thirduser@mail.com"
                    }
                );
            }

            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            app.Logger.LogError(e, "An error occurred while seeding initial data.");
            throw;
        }
    }
}
