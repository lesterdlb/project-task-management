using FluentAssertions;
using ProjectManagement.Api.Core.Domain.Abstractions;
using ProjectManagement.Api.Features.Projects;
using ProjectManagement.Api.UnitTests.Helpers;

namespace ProjectManagement.Api.UnitTests.Features.Projects;

public class GetProjectQueryHandlerTests : ProjectHandlerTestsBase
{
    private readonly GetProject.GetProjectQueryHandler _handler;

    public GetProjectQueryHandlerTests()
    {
        _handler = new GetProject.GetProjectQueryHandler(
            DbContext,
            MockCurrentUserService.Object,
            DataShapingService,
            MockLinkService.Object
        );
    }

    [Fact]
    public async Task Handle_AsOwner_ReturnsProject()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.Id = projectId;
            opts.OwnerId = ownerId;
            opts.Name = "My Project";
        });

        AddProjects(project);
        await SaveChangesAsync();

        SetupMemberUser(ownerId);

        var query = new GetProject.GetProjectQuery(projectId, new GetProject.ProjectQueryParameters());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        IDictionary<string, object?> shapedProject = result.Value!;
        shapedProject["id"].Should().Be(projectId);
        shapedProject["name"].Should().Be("My Project");
    }

    [Fact]
    public async Task Handle_AsMember_ReturnsProject()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.Id = projectId;
            opts.OwnerId = ownerId;
            opts.Name = "Shared Project";
        });

        var projectMember = TestDataFactory.CreateProjectMember(projectId, memberId);

        AddProjects(project);
        AddProjectMembers(projectMember);
        await SaveChangesAsync();

        SetupMemberUser(memberId);

        var query = new GetProject.GetProjectQuery(projectId, new GetProject.ProjectQueryParameters());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        IDictionary<string, object?> shapedProject = result.Value!;
        shapedProject["id"].Should().Be(projectId);
        shapedProject["name"].Should().Be("Shared Project");
    }

    [Fact]
    public async Task Handle_AsAdmin_ReturnsAnyProject()
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

        var query = new GetProject.GetProjectQuery(projectId, new GetProject.ProjectQueryParameters());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        IDictionary<string, object?> shapedProject = result.Value!;
        shapedProject["id"].Should().Be(projectId);
    }

    [Fact]
    public async Task Handle_WithFieldSelection_ReturnsOnlyRequestedFields()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.Id = projectId;
            opts.OwnerId = ownerId;
            opts.Name = "Test Project";
        });

        AddProjects(project);
        await SaveChangesAsync();

        SetupMemberUser(ownerId);

        var query = new GetProject.GetProjectQuery(
            projectId,
            new GetProject.ProjectQueryParameters { Fields = "id,name,status" });

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        IDictionary<string, object?> shapedProject = result.Value!;

        shapedProject.Should().ContainKey("id");
        shapedProject.Should().ContainKey("name");
        shapedProject.Should().ContainKey("status");
        shapedProject.Should().NotContainKey("description");
        shapedProject.Should().NotContainKey("priority");
        shapedProject.Should().NotContainKey("startDate");
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var nonExistentProjectId = Guid.NewGuid();

        SetupMemberUser(userId);

        var query = new GetProject.GetProjectQuery(
            nonExistentProjectId,
            new GetProject.ProjectQueryParameters());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(Error.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenNotOwnerNorMemberNorAdmin_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.Id = projectId;
            opts.OwnerId = ownerId;
            opts.Name = "Someone Else's Project";
        });

        AddProjects(project);
        await SaveChangesAsync();

        SetupMemberUser(userId);

        var query = new GetProject.GetProjectQuery(projectId, new GetProject.ProjectQueryParameters());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

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

        var query = new GetProject.GetProjectQuery(projectId, new GetProject.ProjectQueryParameters());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(Error.Unauthorized.Code);
    }
}
