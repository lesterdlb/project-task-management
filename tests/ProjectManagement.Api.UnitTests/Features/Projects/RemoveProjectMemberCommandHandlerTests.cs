using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Features.Projects;
using ProjectManagement.Api.UnitTests.Helpers;

namespace ProjectManagement.Api.UnitTests.Features.Projects;

public class RemoveProjectMemberCommandHandlerTests : ProjectHandlerTestsBase
{
    private readonly RemoveProjectMember.RemoveProjectMemberCommandHandler _handler;

    public RemoveProjectMemberCommandHandlerTests()
    {
        _handler = new RemoveProjectMember.RemoveProjectMemberCommandHandler(
            DbContext,
            MockCurrentUserService.Object
        );
    }

    [Fact]
    public async Task Handle_AsOwner_RemovesProjectMember()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var owner = TestDataFactory.CreateUser(ownerId, "owner");
        var member = TestDataFactory.CreateUser(memberId, "member");

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.Id = projectId;
            opts.OwnerId = ownerId;
        });

        var projectMember = TestDataFactory.CreateProjectMember(projectId, memberId);

        DbContext.Users.AddRange(owner, member);
        AddProjects(project);
        AddProjectMembers(projectMember);
        await SaveChangesAsync();

        SetupMemberUser(ownerId);

        var command = new RemoveProjectMember.RemoveProjectMemberCommand(projectId, memberId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var removedMember = await DbContext.ProjectMembers
            .Where(pm => pm.ProjectId == projectId && pm.UserId == memberId)
            .SingleOrDefaultAsync();

        removedMember.Should().BeNull();

        var memberCount = await DbContext.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .CountAsync();
        memberCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_AsAdmin_RemovesProjectMemberFromAnyProject()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var admin = TestDataFactory.CreateAdminUser(adminId, "admin");
        var owner = TestDataFactory.CreateUser(ownerId, "owner");
        var member = TestDataFactory.CreateUser(memberId, "member");

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.Id = projectId;
            opts.OwnerId = ownerId;
        });

        var projectMember = TestDataFactory.CreateProjectMember(projectId, memberId);

        DbContext.Users.AddRange(admin, owner, member);
        AddProjects(project);
        AddProjectMembers(projectMember);
        await SaveChangesAsync();

        SetupAdminUser(adminId);

        var command = new RemoveProjectMember.RemoveProjectMemberCommand(projectId, memberId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var removedMember = await DbContext.ProjectMembers
            .Where(pm => pm.ProjectId == projectId && pm.UserId == memberId)
            .SingleOrDefaultAsync();

        removedMember.Should().BeNull();
    }

    [Fact]
    public async Task Handle_AsRegularUser_CannotRemoveMemberFromOthersProject()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var user = TestDataFactory.CreateUser(userId, "user");
        var owner = TestDataFactory.CreateUser(ownerId, "owner");
        var member = TestDataFactory.CreateUser(memberId, "member");

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.Id = projectId;
            opts.OwnerId = ownerId;
        });

        var projectMember = TestDataFactory.CreateProjectMember(projectId, memberId);

        DbContext.Users.AddRange(user, owner, member);
        AddProjects(project);
        AddProjectMembers(projectMember);
        await SaveChangesAsync();

        SetupMemberUser(userId);

        var command = new RemoveProjectMember.RemoveProjectMemberCommand(projectId, memberId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(Error.NotFound.Code);

        var stillExistingMember = await DbContext.ProjectMembers
            .Where(pm => pm.ProjectId == projectId && pm.UserId == memberId)
            .SingleOrDefaultAsync();

        stillExistingMember.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ReturnsNotFound()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var nonExistentProjectId = Guid.NewGuid();

        var owner = TestDataFactory.CreateUser(ownerId, "owner");
        var member = TestDataFactory.CreateUser(memberId, "member");

        DbContext.Users.AddRange(owner, member);
        await SaveChangesAsync();

        SetupMemberUser(ownerId);

        var command = new RemoveProjectMember.RemoveProjectMemberCommand(nonExistentProjectId, memberId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(Error.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenMemberNotFound_ReturnsUserNotFound()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var nonExistentMemberId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var owner = TestDataFactory.CreateUser(ownerId, "owner");

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.Id = projectId;
            opts.OwnerId = ownerId;
        });

        DbContext.Users.Add(owner);
        AddProjects(project);
        await SaveChangesAsync();

        SetupMemberUser(ownerId);

        var command = new RemoveProjectMember.RemoveProjectMemberCommand(projectId, nonExistentMemberId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(Error.ProjectMember.UserNotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        SetupUnauthenticated();

        var command = new RemoveProjectMember.RemoveProjectMemberCommand(projectId, memberId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(Error.Unauthorized.Code);
    }
}
