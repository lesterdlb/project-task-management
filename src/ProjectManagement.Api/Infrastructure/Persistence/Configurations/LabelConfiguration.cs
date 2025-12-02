using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Api.Core.Domain.Entities;

namespace ProjectManagement.Api.Infrastructure.Persistence.Configurations;

public sealed class LabelConfiguration : EntityConfiguration<Label>
{
    public override void Configure(EntityTypeBuilder<Label> builder)
    {
        base.Configure(builder);

        builder.ToTable("Labels");

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(l => l.Color)
            .IsRequired()
            .HasMaxLength(7);

        builder.HasOne(l => l.Project)
            .WithMany(p => p.Labels)
            .HasForeignKey(l => l.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}