using LibraryApi.Domain.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApi.Persistence.Configurations;

public sealed class BookConfiguration : BaseAuditedConfiguration<Book>
{
    protected override void ConfigureDerived(EntityTypeBuilder<Book> b)
    {
        b.ToTable("Books");

        b.Property(book => book.Title).HasMaxLength(200).IsRequired();
        b.Property(book => book.Isbn).HasMaxLength(13).IsRequired();
        b.HasIndex(book => book.Isbn).IsUnique();
    }
}
