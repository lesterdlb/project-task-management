using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Api.Core.Domain.Entities;

namespace ProjectManagement.Api.Infrastructure.Persistence.Configurations;

public sealed class ProjectConfiguration : EntityConfiguration<Project>
{
    public override void Configure(EntityTypeBuilder<Project> builder)
    {
        base.Configure(builder);

        builder.ToTable("Projects");

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
        builder.HasIndex(p => p.Name)
            .IsUnique();
        builder.Property(p => p.Description)
            .HasMaxLength(2000);
        builder.Property(p => p.StartDate)
            .IsRequired();
        builder.Property(p => p.EndDate)
            .IsRequired();
        builder.Property(p => p.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);
        builder.Property(p => p.Priority)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne(p => p.Owner)
            .WithMany(u => u.OwnedProjects)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}