using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Features.Projects;
using ProjectManagement.Api.UnitTests.Helpers;

namespace ProjectManagement.Api.UnitTests.Features.Projects;

public class DeleteProjectCommandHandlerTests : ProjectHandlerTestsBase
{
    private readonly DeleteProject.DeleteProjectCommandHandler _handler;

    public DeleteProjectCommandHandlerTests()
    {
        _handler = new DeleteProject.DeleteProjectCommandHandler(
            DbContext,
            MockCurrentUserService.Object
        );
    }

    [Fact]
    public async Task Handle_AsOwner_DeletesProject()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.Id = projectId;
            opts.OwnerId = ownerId;
            opts.Name = "Project To Delete";
        });

        AddProjects(project);
        await SaveChangesAsync();

        SetupMemberUser(ownerId);

        var command = new DeleteProject.DeleteProjectCommand(projectId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deletedProject = await DbContext.Projects.FindAsync(projectId);
        deletedProject.Should().BeNull();

        var projectCount = await DbContext.Projects.CountAsync();
        projectCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_AsAdmin_DeletesAnyProject()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.Id = projectId;
            opts.OwnerId = ownerId;
            opts.Name = "Other User's Project";
        });

        AddProjects(project);
        await SaveChangesAsync();

        SetupAdminUser(adminId);

        var command = new DeleteProject.DeleteProjectCommand(projectId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deletedProject = await DbContext.Projects.FindAsync(projectId);
        deletedProject.Should().BeNull();
    }

    [Fact]
    public async Task Handle_AsRegularUser_CannotDeleteOthersProject()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.Id = projectId;
            opts.OwnerId = ownerId;
            opts.Name = "Protected Project";
        });

        AddProjects(project);
        await SaveChangesAsync();

        SetupMemberUser(userId);

        var command = new DeleteProject.DeleteProjectCommand(projectId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(Error.NotFound.Code);

        var stillExistingProject = await DbContext.Projects.FindAsync(projectId);
        stillExistingProject.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var nonExistentProjectId = Guid.NewGuid();

        SetupMemberUser(userId);

        var command = new DeleteProject.DeleteProjectCommand(nonExistentProjectId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(Error.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var projectId = Guid.NewGuid();

        SetupUnauthenticated();

        var command = new DeleteProject.DeleteProjectCommand(projectId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(Error.Unauthorized.Code);
    }

    [Fact]
    public async Task Handle_WithProjectMembers_DeletesProject()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.Id = projectId;
            opts.OwnerId = ownerId;
            opts.Name = "Project With Members";
        });

        var projectMember = TestDataFactory.CreateProjectMember(projectId, memberId);

        AddProjects(project);
        AddProjectMembers(projectMember);
        await SaveChangesAsync();

        SetupMemberUser(ownerId);

        var command = new DeleteProject.DeleteProjectCommand(projectId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deletedProject = await DbContext.Projects.FindAsync(projectId);
        deletedProject.Should().BeNull();

        var memberCount = await DbContext.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .CountAsync();
        memberCount.Should().Be(0);
    }
}
