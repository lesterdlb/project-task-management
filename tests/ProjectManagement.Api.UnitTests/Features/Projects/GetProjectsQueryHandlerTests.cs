using FluentAssertions;
using ProjectManagement.Api.Common.Domain.Abstractions;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Features.Projects;
using ProjectManagement.Api.UnitTests.Helpers;

namespace ProjectManagement.Api.UnitTests.Features.Projects;

public class GetProjectsQueryHandlerTests : ProjectHandlerTestsBase
{
    private readonly GetProjects.GetProjectsQueryHandler _handler;

    public GetProjectsQueryHandlerTests()
    {
        _handler = new GetProjects.GetProjectsQueryHandler(
            DbContext,
            MockCurrentUserService.Object,
            SortMappingProvider,
            DataShapingService,
            MockLinkService.Object
        );
    }

    [Fact]
    public async Task Handle_AsAdmin_ReturnsAllProjects()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();

        var projects = new List<Project>
        {
            TestDataFactory.CreateProject(opts =>
            {
                opts.OwnerId = user1Id;
                opts.Name = "Project 1";
            }),
            TestDataFactory.CreateProject(opts =>
            {
                opts.OwnerId = user2Id;
                opts.Name = "Project 2";
            }),
            TestDataFactory.CreateProject(opts =>
            {
                opts.OwnerId = adminId;
                opts.Name = "Project 3";
            })
        };

        AddProjects([.. projects]);
        await SaveChangesAsync();

        SetupAdminUser(adminId);

        var query = new GetProjects.GetProjectsQuery(new GetProjects.ProjectsQueryParameters
        {
            Page = 1,
            PageSize = 10
        });

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
        result.Value.TotalCount.Should().Be(3);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_AsMember_ReturnsOnlyOwnedAndMemberProjects()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var project1Id = Guid.NewGuid();
        var project2Id = Guid.NewGuid();
        var project3Id = Guid.NewGuid();

        var projects = new List<Project>
        {
            TestDataFactory.CreateProject(opts =>
            {
                opts.Id = project1Id;
                opts.OwnerId = memberId;
                opts.Name = "My Project";
            }),
            TestDataFactory.CreateProject(opts =>
            {
                opts.Id = project2Id;
                opts.OwnerId = otherUserId;
                opts.Name = "Other's Project";
            }),
            TestDataFactory.CreateProject(opts =>
            {
                opts.Id = project3Id;
                opts.OwnerId = otherUserId;
                opts.Name = "Shared Project";
            })
        };

        var projectMember = TestDataFactory.CreateProjectMember(project3Id, memberId);

        AddProjects([.. projects]);
        AddProjectMembers(projectMember);
        await SaveChangesAsync();

        SetupMemberUser(memberId);

        var query = new GetProjects.GetProjectsQuery(new GetProjects.ProjectsQueryParameters
        {
            Page = 1,
            PageSize = 10
        });

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);

        var projectNames = result.Value.Items
            .Select(p => ((IDictionary<string, object?>)p)["name"])
            .Cast<string>()
            .ToList();

        projectNames.Should().Contain("My Project");
        projectNames.Should().Contain("Shared Project");
        projectNames.Should().NotContain("Other's Project");
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userOtherMemberId = Guid.NewGuid();

        var projects = TestDataFactory.CreateProjects(25, opts => opts.OwnerId = userId);
        var memberProject = TestDataFactory.CreateProjects(10, opts => opts.OwnerId = userOtherMemberId);

        AddProjects([.. projects]);
        AddProjects([.. memberProject]);
        await SaveChangesAsync();

        SetupAdminUser(userId);

        var query = new GetProjects.GetProjectsQuery(new GetProjects.ProjectsQueryParameters
        {
            Page = 2,
            PageSize = 10
        });

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(10);
        result.Value.TotalCount.Should().Be(35);
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(10);
        result.Value.HasNextPage.Should().BeTrue();
        result.Value.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithSorting_ReturnsSortedResults()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var projects = new List<Project>
        {
            TestDataFactory.CreateProject(opts =>
            {
                opts.OwnerId = userId;
                opts.Name = "A Project";
            }),
            TestDataFactory.CreateProject(opts =>
            {
                opts.OwnerId = userId;
                opts.Name = "C Project";
            }),
            TestDataFactory.CreateProject(opts =>
            {
                opts.OwnerId = userId;
                opts.Name = "B Project";
            })
        };

        AddProjects([.. projects]);
        await SaveChangesAsync();

        SetupAdminUser(userId);

        var query = new GetProjects.GetProjectsQuery(new GetProjects.ProjectsQueryParameters
        {
            Page = 1,
            PageSize = 10,
            Sort = "name desc"
        });

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);

        var projectNames = result.Value.Items
            .Select(p => ((IDictionary<string, object?>)p)["name"])
            .Cast<string>()
            .ToList();

        projectNames[0].Should().Be("C Project");
        projectNames[1].Should().Be("B Project");
        projectNames[2].Should().Be("A Project");
    }

    [Fact]
    public async Task Handle_WithFieldSelection_ReturnsOnlyRequestedFields()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var project = TestDataFactory.CreateProject(opts =>
        {
            opts.OwnerId = userId;
            opts.Name = "Test Project";
        });

        AddProjects(project);
        await SaveChangesAsync();

        SetupAdminUser(userId);

        var query = new GetProjects.GetProjectsQuery(new GetProjects.ProjectsQueryParameters
        {
            Page = 1,
            PageSize = 10,
            Fields = "id,name"
        });

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);

        IDictionary<string, object?> shapedProject = result.Value.Items[0];

        shapedProject.Should().ContainKey("id");
        shapedProject.Should().ContainKey("name");
        shapedProject.Should().NotContainKey("description");
        shapedProject.Should().NotContainKey("status");
        shapedProject.Should().NotContainKey("priority");
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ReturnsUnauthorizedError()
    {
        // Arrange
        SetupUnauthenticated();

        var query = new GetProjects.GetProjectsQuery(new GetProjects.ProjectsQueryParameters
        {
            Page = 1,
            PageSize = 10
        });

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(Error.Unauthorized.Code);
    }

    [Fact]
    public async Task Handle_WithEmptyDatabase_ReturnsEmptyCollection()
    {
        // Arrange
        var userId = Guid.NewGuid();

        SetupAdminUser(userId);

        var query = new GetProjects.GetProjectsQuery(new GetProjects.ProjectsQueryParameters());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }
}
