using ProjectManagement.Api.Core.Domain.Entities;
using ProjectManagement.Api.Core.Domain.Enums;

namespace ProjectManagement.Api.UnitTests.Helpers;

public static class TestDataFactory
{
    public static Project CreateProject(Action<ProjectOptions>? configure = null)
    {
        var options = new ProjectOptions();
        configure?.Invoke(options);

        return new Project
        {
            Id = options.Id ?? Guid.NewGuid(),
            Name = options.Name ?? "Test Project",
            Description = options.Description ?? "Test Description",
            OwnerId = options.OwnerId ?? Guid.NewGuid(),
            StartDate = options.StartDate ?? DateTime.UtcNow.AddDays(1),
            EndDate = options.EndDate ?? DateTime.UtcNow.AddDays(30),
            Status = options.Status ?? ProjectStatus.Active,
            Priority = options.Priority ?? Priority.Medium,
            CreatedBy = options.OwnerId ?? Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static List<Project> CreateProjects(int count, Action<ProjectOptions>? configure = null)
    {
        var projects = new List<Project>();
        for (var i = 1; i <= count; i++)
        {
            var index = i;
            projects.Add(CreateProject(opts =>
            {
                opts.Name = $"Project {index}";
                configure?.Invoke(opts);
            }));
        }

        return projects;
    }

    public static ProjectMember CreateProjectMember(
        Guid projectId,
        Guid userId,
        ProjectRole role = ProjectRole.Contributor)
    {
        return new ProjectMember
        {
            ProjectId = projectId,
            UserId = userId,
            RoleInProject = role,
            DateJoined = DateTime.UtcNow
        };
    }

    public static User CreateUser(
        Guid? id = null,
        string? userName = null,
        string? email = null,
        UserRole role = UserRole.Member)
    {
        var userId = id ?? Guid.NewGuid();
        var userNameValue = userName ?? "testuser";

        return new User
        {
            Id = userId,
            UserName = userNameValue,
            Email = email ?? $"{userNameValue}@test.com",
            FullName = userName ?? "Test User",
            Role = role,
            EmailConfirmed = true,
            CreatedBy = userId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static User CreateAdminUser(Guid? id = null, string? userName = null)
    {
        return CreateUser(id, userName ?? "admin", role: UserRole.Admin);
    }
}

public class ProjectOptions
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid? OwnerId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ProjectStatus? Status { get; set; }
    public Priority? Priority { get; set; }
}
