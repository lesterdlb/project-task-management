using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Api.Common.Domain.Entities;

namespace ProjectManagement.Api.Common.Persistence.Configurations;

public sealed class ProjectTaskConfiguration : EntityConfiguration<ProjectTask>
{
    public override void Configure(EntityTypeBuilder<ProjectTask> builder)
    {
        base.Configure(builder);

        builder.ToTable("Tasks");

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(300);
        builder.HasIndex(t => t.Title)
            .IsUnique();
        builder.Property(t => t.Description)
            .HasMaxLength(5000);
        builder.Property(t => t.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);
        builder.Property(t => t.Priority)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);
        builder.Property(t => t.DueDate)
            .IsRequired();

        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.AssignedTo)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<User>()
            .WithMany(u => u.CreatedTasks)
            .HasForeignKey(pt => pt.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}