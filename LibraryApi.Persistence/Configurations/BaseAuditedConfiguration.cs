using LibraryApi.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApi.Persistence.Configurations;

/// <summary>
/// Base configuration for entities with auditing fields.
/// Automatically configures CreatedAtUtc and UpdatedAtUtc.
/// </summary>
public abstract class BaseAuditedConfiguration<T> : IEntityTypeConfiguration<T> where T : AuditedEntity
{
    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CreatedAtUtc).IsRequired();
        builder.Property(e => e.UpdatedAtUtc).IsRequired(false);

        ConfigureDerived(builder);
    }

    protected abstract void ConfigureDerived(EntityTypeBuilder<T> builder);
}
