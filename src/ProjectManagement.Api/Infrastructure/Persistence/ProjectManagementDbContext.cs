using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Api.Core.Domain;
using ProjectManagement.Api.Core.Domain.Entities;
using ProjectManagement.Api.Core.Application.Services.Auth;

namespace ProjectManagement.Api.Infrastructure.Persistence;

public sealed class ProjectManagementDbContext(
    ICurrentUserService currentUserService,
    DbContextOptions<ProjectManagementDbContext> options)
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();
    public DbSet<Label> Labels => Set<Label>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.Property(u => u.FullName).IsRequired();
            entity.Property(u => u.Role).IsRequired();
            entity.Property(u => u.Version).IsRowVersion();
            entity.ToTable(name: "Users");
        });
        builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        builder.ApplyConfigurationsFromAssembly(typeof(ProjectManagementDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var currentUserId = currentUserService.UserId ?? Guid.Empty;

        foreach (var entry in ChangeTracker.Entries<User>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = currentUserId;
                    entry.Entity.LastModifiedBy = null;
                    entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                    entry.Entity.UpdatedAtUtc = null;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedBy = currentUserId;
                    entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = currentUserId;
                    entry.Entity.LastModifiedBy = null;
                    entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                    entry.Entity.UpdatedAtUtc = null;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedBy = currentUserId;
                    entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
