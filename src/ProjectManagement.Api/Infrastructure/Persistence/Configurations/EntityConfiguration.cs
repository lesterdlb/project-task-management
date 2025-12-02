using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Api.Core.Domain;

namespace ProjectManagement.Api.Infrastructure.Persistence.Configurations;

public abstract class EntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(100);

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();

        builder.Property(e => e.Version)
            .IsRowVersion();
    }
}
