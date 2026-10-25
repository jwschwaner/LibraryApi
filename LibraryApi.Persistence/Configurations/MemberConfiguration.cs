using LibraryApi.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApi.Persistence.Configurations;

public sealed class MemberConfiguration : BaseAuditedConfiguration<Member>
{
    protected override void ConfigureDerived(EntityTypeBuilder<Member> b)
    {
        b.ToTable("Members");

        b.Property(m => m.FullName).HasMaxLength(200).IsRequired();
        b.Property(m => m.Email).HasMaxLength(254).IsRequired();
        b.HasIndex(m => m.Email).IsUnique();
    }
}
