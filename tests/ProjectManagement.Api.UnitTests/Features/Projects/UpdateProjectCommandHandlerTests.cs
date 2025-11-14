using FluentAssertions;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.Domain.Enums;
using ProjectManagement.Api.Features.Projects;
using ProjectManagement.Api.UnitTests.Helpers;

namespace ProjectManagement.Api.UnitTests.Features.Projects;

public class UpdateProjectCommandHandlerTests : ProjectHandlerTestsBase
{
    private readonly UpdateProject.UpdateProjectCommandHandler _handler;

    public UpdateProjectCommandHandlerTests()
    {
        _handler = new UpdateProject.UpdateProjectCommandHandler(
            DbContext,
            MockCurrentUserService.Object
        );
    }

    [Fact]
    public async Task Handle_AsOwner_UpdatesProject()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.Id = projectId;
            opts.OwnerId = ownerId;
            opts.Name = "Original Name";
            opts.Description = "Original Description";
            opts.Status = ProjectStatus.Active;
        });

        AddProjects(project);
        await SaveChangesAsync();

        SetupMemberUser(ownerId);

        var dto = new UpdateProject.UpdateProjectDto(
            Name: "Updated Name",
            Description: "Updated Description",
            StartDate: DateTime.UtcNow.AddDays(2),
            EndDate: DateTime.UtcNow.AddDays(40),
            Status: ProjectStatus.Archived,
            Priority: Priority.High
        );

        var command = new UpdateProject.UpdateProjectCommand(projectId, dto);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updatedProject = await DbContext.Projects.FindAsync(projectId);
        updatedProject.Should().NotBeNull();
        updatedProject!.Name.Should().Be("Updated Name");
        updatedProject.Description.Should().Be("Updated Description");
        updatedProject.Status.Should().Be(ProjectStatus.Archived);
        updatedProject.Priority.Should().Be(Priority.High);
    }

    [Fact]
    public async Task Handle_AsAdmin_UpdatesAnyProject()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.Id = projectId;
            opts.OwnerId = ownerId;
            opts.Name = "Original Project";
        });

        AddProjects(project);
        await SaveChangesAsync();

        SetupAdminUser(adminId);

        var dto = new UpdateProject.UpdateProjectDto(
            Name: "Admin Updated",
            Description: "Updated by admin",
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(30),
            Status: ProjectStatus.Completed,
            Priority: Priority.Low
        );

        var command = new UpdateProject.UpdateProjectCommand(projectId, dto);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updatedProject = await DbContext.Projects.FindAsync(projectId);
        updatedProject.Should().NotBeNull();
        updatedProject.Name.Should().Be("Admin Updated");
        updatedProject.Status.Should().Be(ProjectStatus.Completed);
    }

    [Fact]
    public async Task Handle_AsRegularUser_CannotUpdateOthersProject()
    {
        // Arrange
        var userId = Guid.NewGuid();
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

        SetupMemberUser(userId);

        var dto = new UpdateProject.UpdateProjectDto(
            Name: "Attempted Update",
            Description: "Description",
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(30),
            Status: ProjectStatus.Active,
            Priority: Priority.Medium
        );

        var command = new UpdateProject.UpdateProjectCommand(projectId, dto);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(Error.NotFound.Code);

        var unchangedProject = await DbContext.Projects.FindAsync(projectId);
        unchangedProject!.Name.Should().Be("Other User's Project");
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var nonExistentProjectId = Guid.NewGuid();

        SetupMemberUser(userId);

        var dto = new UpdateProject.UpdateProjectDto(
            Name: "Updated Name",
            Description: "Description",
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(30),
            Status: ProjectStatus.Active,
            Priority: Priority.Medium
        );

        var command = new UpdateProject.UpdateProjectCommand(nonExistentProjectId, dto);

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

        var dto = new UpdateProject.UpdateProjectDto(
            Name: "Updated Name",
            Description: "Description",
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(30),
            Status: ProjectStatus.Active,
            Priority: Priority.Medium
        );

        var command = new UpdateProject.UpdateProjectCommand(projectId, dto);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(Error.Unauthorized.Code);
    }

    [Fact]
    public async Task Validate_EndDate_MustBeAfterStartDate()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(5);

        var dto = new UpdateProject.UpdateProjectDto(
            Name: "Project",
            Description: "Description",
            StartDate: startDate,
            EndDate: endDate,
            Status: ProjectStatus.Active,
            Priority: Priority.Medium
        );

        var command = new UpdateProject.UpdateProjectCommand(Guid.NewGuid(), dto);

        var validator = new UpdateProject.UpdateProjectCommandValidator();

        // Act
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.PropertyName == "Dto.EndDate");
    }
}
