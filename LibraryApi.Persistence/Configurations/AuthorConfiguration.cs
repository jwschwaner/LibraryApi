using LibraryApi.Domain.Authors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApi.Persistence.Configurations;

public sealed class AuthorConfiguration : BaseAuditedConfiguration<Author>
{
    protected override void ConfigureDerived(EntityTypeBuilder<Author> b)
    {
        b.ToTable("Authors");

        b.Property(a => a.FirstName).HasMaxLength(100).IsRequired();
        b.Property(a => a.LastName).HasMaxLength(100).IsRequired();

        // An author with books can't be deleted; the handler checks first and returns 409
        b.HasMany(a => a.Books)
            .WithOne()
            .HasForeignKey(book => book.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        b.Navigation(a => a.Books).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
