using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.Domain.Enums;
using ProjectManagement.Api.Features.Projects;
using ProjectManagement.Api.UnitTests.Helpers;

namespace ProjectManagement.Api.UnitTests.Features.Projects;

public class CreateProjectCommandHandlerTests : ProjectHandlerTestsBase
{
    private readonly CreateProject.CreateProjectCommandHandler _handler;

    public CreateProjectCommandHandlerTests()
    {
        _handler = new CreateProject.CreateProjectCommandHandler(
            DbContext,
            MockCurrentUserService.Object,
            MockLinkService.Object
        );
    }

    [Fact]
    public async Task Handle_AsRegularUser_CreatesProjectForSelf()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = TestDataFactory.CreateUser(userId, "regularuser");

        DbContext.Users.Add(user);
        await SaveChangesAsync();

        SetupMemberUser(userId);

        var dto = new CreateProject.CreateProjectDto(
            Name: "New Project",
            Description: "Project description",
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(30),
            Status: ProjectStatus.Active,
            Priority: Priority.Medium,
            OwnerId: null
        );

        var command = new CreateProject.CreateProjectCommand(dto);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ProjectDto.Name.Should().Be("New Project");
        result.Value.ProjectDto.Id.Should().NotBeEmpty();

        var savedProject = await DbContext.Projects.FindAsync(result.Value.ProjectDto.Id);
        savedProject.Should().NotBeNull();
        savedProject.OwnerId.Should().Be(userId);
        savedProject.Name.Should().Be("New Project");
    }

    [Fact]
    public async Task Handle_AsAdmin_CreatesProjectForOtherUser()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        var admin = TestDataFactory.CreateAdminUser(adminId, "admin");
        var targetUser = TestDataFactory.CreateUser(targetUserId, "targetuser");

        DbContext.Users.AddRange(admin, targetUser);
        await SaveChangesAsync();

        SetupAdminUser(adminId);

        var dto = new CreateProject.CreateProjectDto(
            Name: "Delegated Project",
            Description: "Project for another user",
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(30),
            Status: ProjectStatus.Active,
            Priority: Priority.High,
            OwnerId: targetUserId
        );

        var command = new CreateProject.CreateProjectCommand(dto);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ProjectDto.Name.Should().Be("Delegated Project");

        var savedProject = await DbContext.Projects.FindAsync(result.Value.ProjectDto.Id);
        savedProject.Should().NotBeNull();
        savedProject.OwnerId.Should().Be(targetUserId);
    }

    [Fact]
    public async Task Handle_AsRegularUserWithOwnerId_ReturnsCreationForbidden()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var user = TestDataFactory.CreateUser(userId, "regularuser");
        var otherUser = TestDataFactory.CreateUser(otherUserId, "otheruser");

        DbContext.Users.AddRange(user, otherUser);
        await SaveChangesAsync();

        SetupMemberUser(userId);

        var dto = new CreateProject.CreateProjectDto(
            Name: "Forbidden Project",
            Description: "Trying to create for another user",
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(30),
            Status: ProjectStatus.Active,
            Priority: Priority.Medium,
            OwnerId: otherUserId
        );

        var command = new CreateProject.CreateProjectCommand(dto);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(Error.Project.CreationForbidden.Code);

        var projectCount = await DbContext.Projects.CountAsync();
        projectCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenOwnerNotFound_ReturnsOwnerNotFound()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var nonExistentUserId = Guid.NewGuid();

        var admin = TestDataFactory.CreateAdminUser(adminId, "admin");

        DbContext.Users.Add(admin);
        await SaveChangesAsync();

        SetupAdminUser(adminId);

        var dto = new CreateProject.CreateProjectDto(
            Name: "Project",
            Description: "Description",
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(30),
            Status: ProjectStatus.Active,
            Priority: Priority.Medium,
            OwnerId: nonExistentUserId
        );

        var command = new CreateProject.CreateProjectCommand(dto);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(Error.Project.OwnerNotFound.Code);

        var projectCount = await DbContext.Projects.CountAsync();
        projectCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        SetupUnauthenticated();

        var dto = new CreateProject.CreateProjectDto(
            Name: "Project",
            Description: "Description",
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(30),
            Status: ProjectStatus.Active,
            Priority: Priority.Medium
        );

        var command = new CreateProject.CreateProjectCommand(dto);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(Error.Unauthorized.Code);

        var projectCount = await DbContext.Projects.CountAsync();
        projectCount.Should().Be(0);
    }

    [Fact]
    public async Task Validate_StartDate_MustBeInFuture()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = TestDataFactory.CreateUser(userId, "user");

        DbContext.Users.Add(user);
        await SaveChangesAsync();

        SetupMemberUser(userId);

        var dto = new CreateProject.CreateProjectDto(
            Name: "Project",
            Description: "Description",
            StartDate: DateTime.UtcNow.AddDays(-1),
            EndDate: DateTime.UtcNow.AddDays(30),
            Status: ProjectStatus.Active,
            Priority: Priority.Medium
        );

        var command = new CreateProject.CreateProjectCommand(dto);

        var validator = new CreateProject.CreateProjectCommandValidator();

        // Act
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.PropertyName == "Dto.StartDate");
    }

    [Fact]
    public async Task Validate_EndDate_MustBeAfterStartDate()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(5);

        var dto = new CreateProject.CreateProjectDto(
            Name: "Project",
            Description: "Description",
            StartDate: startDate,
            EndDate: endDate,
            Status: ProjectStatus.Active,
            Priority: Priority.Medium
        );

        var command = new CreateProject.CreateProjectCommand(dto);

        var validator = new CreateProject.CreateProjectCommandValidator();

        // Act
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.PropertyName == "Dto.EndDate");
    }
}
