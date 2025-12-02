using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Api.Core.Domain.Entities;

namespace ProjectManagement.Api.Infrastructure.Persistence.Configurations;

public sealed class CommentConfiguration : EntityConfiguration<Comment>
{
    public override void Configure(EntityTypeBuilder<Comment> builder)
    {
        base.Configure(builder);

        builder.ToTable("Comments");

        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(5000);

        builder.HasOne(c => c.Author)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.ProjectTask)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.ProjectTaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}