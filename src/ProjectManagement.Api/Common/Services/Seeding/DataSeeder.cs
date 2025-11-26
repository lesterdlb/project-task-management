using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.Domain.Enums;
using ProjectManagement.Api.Common.Persistence;

namespace ProjectManagement.Api.Common.Services.Seeding;

public class DataSeeder(
    ProjectManagementDbContext dbContext,
    UserManager<User> userManager,
    ILogger<DataSeeder> logger)
{
    public async Task SeedDummyDataAsync(int userCount, int projectCount)
    {
        if (await dbContext.Projects.AnyAsync())
        {
            logger.LogInformation("Dummy data already exists. Skipping seeding.");
            return;
        }

        logger.LogInformation("Starting dummy data seeding...");

        var users = await SeedDummyUsersAsync(userCount);
        await SeedProjectsAsync(users, projectCount);

        logger.LogInformation("Dummy data seeding completed.");
    }

    private async Task<List<User>> SeedDummyUsersAsync(int count)
    {
        var userFaker = new Faker<User>()
            .RuleFor(u => u.UserName, f => f.Internet.UserName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.FullName, f => f.Name.FullName())
            .RuleFor(u => u.Role, f => f.PickRandomWithout(UserRole.Admin))
            .RuleFor(u => u.EmailConfirmed, true)
            .RuleFor(u => u.CreatedBy, Guid.Empty)
            .RuleFor(u => u.CreatedAtUtc, f => f.Date.Past(2));

        var users = new List<User>();
        const string genericPassword = "P@ssW0rd1!";
        foreach (var user in userFaker.Generate(count))
        {
            var result = await userManager.CreateAsync(user, genericPassword);
            if (!result.Succeeded)
            {
                continue;
            }

            await userManager.AddToRoleAsync(user, user.Role.ToString());
            users.Add(user);
        }

        return users;
    }

    private async Task<List<Project>> SeedProjectsAsync(List<User> users, int count)
    {
        var projectFaker = new Faker<Project>()
            .RuleFor(p => p.Id, _ => Guid.NewGuid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Lorem.Paragraph())
            .RuleFor(p => p.Status, f => f.PickRandom<ProjectStatus>())
            .RuleFor(p => p.Priority, f => f.PickRandom<Priority>())
            .RuleFor(p => p.OwnerId, f => f.PickRandom(users).Id)
            .RuleFor(p => p.StartDate, f => DateTime.SpecifyKind(f.Date.Recent(60), DateTimeKind.Utc))
            .RuleFor(p => p.EndDate, (f, p) => DateTime.SpecifyKind(f.Date.Future(1, p.StartDate), DateTimeKind.Utc))
            .RuleFor(p => p.CreatedBy, Guid.Empty)
            .RuleFor(p => p.CreatedAtUtc, f => f.Date.Past());

        var projects = projectFaker.Generate(count);
        await dbContext.Projects.AddRangeAsync(projects);
        await dbContext.SaveChangesAsync();

        return projects;
    }
}
