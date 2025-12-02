using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Core.Domain.Abstractions;
using ProjectManagement.Api.Core.Domain.Enums;
using ProjectManagement.Api.Features.Projects;
using ProjectManagement.Api.UnitTests.Helpers;

namespace ProjectManagement.Api.UnitTests.Features.Projects;

public class AddProjectMemberCommandHandlerTests : ProjectHandlerTestsBase
{
    private readonly AddProjectMember.AddProjectMemberCommandHandler _handler;

    public AddProjectMemberCommandHandlerTests()
    {
        _handler = new AddProjectMember.AddProjectMemberCommandHandler(
            DbContext,
            MockCurrentUserService.Object
        );
    }

    [Fact]
    public async Task Handle_AsOwner_AddsProjectMember()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var owner = TestDataFactory.CreateUser(ownerId, "owner");
        var user = TestDataFactory.CreateUser(userId, "member");

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.Id = projectId;
            opts.OwnerId = ownerId;
        });

        DbContext.Users.AddRange(owner, user);
        AddProjects(project);
        await SaveChangesAsync();

        SetupMemberUser(ownerId);

        var dto = new AddProjectMember.AddProjectMemberDto(userId, ProjectRole.Contributor);
        var command = new AddProjectMember.AddProjectMemberCommand(projectId, dto);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var projectMember = await DbContext.ProjectMembers
            .Where(pm => pm.ProjectId == projectId && pm.UserId == userId)
            .SingleOrDefaultAsync();

        projectMember.Should().NotBeNull();
        projectMember!.RoleInProject.Should().Be(ProjectRole.Contributor);
        projectMember.DateJoined.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_AsAdmin_AddsProjectMemberToAnyProject()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var admin = TestDataFactory.CreateAdminUser(adminId, "admin");
        var owner = TestDataFactory.CreateUser(ownerId, "owner");
        var user = TestDataFactory.CreateUser(userId, "member");

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.Id = projectId;
            opts.OwnerId = ownerId;
        });

        DbContext.Users.AddRange(admin, owner, user);
        AddProjects(project);
        await SaveChangesAsync();

        SetupAdminUser(adminId);

        var dto = new AddProjectMember.AddProjectMemberDto(userId, ProjectRole.Manager);
        var command = new AddProjectMember.AddProjectMemberCommand(projectId, dto);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var projectMember = await DbContext.ProjectMembers
            .Where(pm => pm.ProjectId == projectId && pm.UserId == userId)
            .SingleOrDefaultAsync();

        projectMember.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_AsRegularUser_CannotAddMemberToOthersProject()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var memberToAddId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var user = TestDataFactory.CreateUser(userId, "user");
        var owner = TestDataFactory.CreateUser(ownerId, "owner");
        var memberToAdd = TestDataFactory.CreateUser(memberToAddId, "newmember");

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.Id = projectId;
            opts.OwnerId = ownerId;
        });

        DbContext.Users.AddRange(user, owner, memberToAdd);
        AddProjects(project);
        await SaveChangesAsync();

        SetupMemberUser(userId);

        var dto = new AddProjectMember.AddProjectMemberDto(memberToAddId);
        var command = new AddProjectMember.AddProjectMemberCommand(projectId, dto);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(Error.NotFound.Code);

        var memberCount = await DbContext.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .CountAsync();
        memberCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ReturnsNotFound()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var nonExistentProjectId = Guid.NewGuid();

        var owner = TestDataFactory.CreateUser(ownerId, "owner");
        var user = TestDataFactory.CreateUser(userId, "member");

        DbContext.Users.AddRange(owner, user);
        await SaveChangesAsync();

        SetupMemberUser(ownerId);

        var dto = new AddProjectMember.AddProjectMemberDto(userId);
        var command = new AddProjectMember.AddProjectMemberCommand(nonExistentProjectId, dto);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(Error.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsUserNotFound()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var nonExistentUserId = Guid.NewGuid();
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

        var dto = new AddProjectMember.AddProjectMemberDto(nonExistentUserId);
        var command = new AddProjectMember.AddProjectMemberCommand(projectId, dto);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(ProjectMemberErrors.UserNotFound.Code);

        var memberCount = await DbContext.ProjectMembers.CountAsync();
        memberCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyMember_ReturnsAlreadyMember()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var owner = TestDataFactory.CreateUser(ownerId, "owner");
        var user = TestDataFactory.CreateUser(userId, "member");

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.Id = projectId;
            opts.OwnerId = ownerId;
        });

        var existingMember = TestDataFactory.CreateProjectMember(projectId, userId);

        DbContext.Users.AddRange(owner, user);
        AddProjects(project);
        AddProjectMembers(existingMember);
        await SaveChangesAsync();

        SetupMemberUser(ownerId);

        var dto = new AddProjectMember.AddProjectMemberDto(userId);
        var command = new AddProjectMember.AddProjectMemberCommand(projectId, dto);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(ProjectMemberErrors.AlreadyMember.Code);

        var memberCount = await DbContext.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .CountAsync();
        memberCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenOwnerAsUser_ReturnsOwnerAsMember()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
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

        var dto = new AddProjectMember.AddProjectMemberDto(ownerId);
        var command = new AddProjectMember.AddProjectMemberCommand(projectId, dto);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(ProjectMemberErrors.OwnerAsMember.Code);

        var memberCount = await DbContext.ProjectMembers.CountAsync();
        memberCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        SetupUnauthenticated();

        var dto = new AddProjectMember.AddProjectMemberDto(userId);
        var command = new AddProjectMember.AddProjectMemberCommand(projectId, dto);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(Error.Unauthorized.Code);
    }
}
