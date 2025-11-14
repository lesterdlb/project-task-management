using Microsoft.EntityFrameworkCore;
using Moq;
using ProjectManagement.Api.Common.Domain.Entities;
using ProjectManagement.Api.Common.Mappings;
using ProjectManagement.Api.Common.Models;
using ProjectManagement.Api.Common.Persistence;
using ProjectManagement.Api.Common.Services.Auth;
using ProjectManagement.Api.Common.Services.DataShaping;
using ProjectManagement.Api.Common.Services.Links;
using ProjectManagement.Api.Common.Services.Sorting;

namespace ProjectManagement.Api.UnitTests.Features.Projects;

public abstract class ProjectHandlerTestsBase : IDisposable
{
    protected readonly ProjectManagementDbContext DbContext;
    protected readonly IDataShapingService DataShapingService;
    protected readonly ISortMappingProvider SortMappingProvider;
    protected readonly Mock<ICurrentUserService> MockCurrentUserService;
    protected readonly Mock<ILinkService> MockLinkService;

    protected ProjectHandlerTestsBase()
    {
        MockCurrentUserService = new Mock<ICurrentUserService>();

        var options = new DbContextOptionsBuilder<ProjectManagementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        DbContext = new ProjectManagementDbContext(MockCurrentUserService.Object, options);

        DataShapingService = new DataShapingService();

        var sortMappingDefinitions = new List<ISortMappingDefinition>
        {
            ProjectMappings.ProjectSortMapping
        };
        SortMappingProvider = new SortMappingProvider(sortMappingDefinitions);

        MockLinkService = new Mock<ILinkService>();
        SetupDefaultLinkServiceMock();
    }

    private void SetupDefaultLinkServiceMock()
    {
        MockLinkService
            .Setup(s => s.CreateLinksForCollection(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ExtendedQueryParameters>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns([]);

        MockLinkService
            .Setup(s => s.CreateLinksForItem(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>()))
            .Returns([]);
    }

    protected void SetupAdminUser(Guid adminId)
    {
        MockCurrentUserService.Setup(s => s.UserId).Returns(adminId);
        MockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(true);
    }

    protected void SetupMemberUser(Guid userId)
    {
        MockCurrentUserService.Setup(s => s.UserId).Returns(userId);
        MockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(false);
    }

    protected void SetupUnauthenticated()
    {
        MockCurrentUserService.Setup(s => s.UserId).Returns((Guid?)null);
        MockCurrentUserService.Setup(s => s.IsInRole(It.IsAny<string>())).Returns(false);
    }

    protected void AddProjects(params Project[] projects)
    {
        DbContext.Projects.AddRange(projects);
    }

    protected void AddProjectMembers(params ProjectMember[] members)
    {
        DbContext.ProjectMembers.AddRange(members);
    }

    protected async Task SaveChangesAsync()
    {
        await DbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        DbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
