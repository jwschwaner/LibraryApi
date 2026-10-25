using LibraryApi.Domain.Books;
using LibraryApi.Domain.Loans;
using LibraryApi.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApi.Persistence.Configurations;

public sealed class LoanConfiguration : BaseAuditedConfiguration<Loan>
{
    protected override void ConfigureDerived(EntityTypeBuilder<Loan> b)
    {
        b.ToTable("Loans");

        b.Property(l => l.BorrowedAtUtc).IsRequired();
        b.Property(l => l.DueOn).IsRequired();
        b.Property(l => l.ReturnedAtUtc).IsRequired(false);
        b.Ignore(l => l.IsReturned);

        b.HasOne<Book>()
            .WithMany()
            .HasForeignKey(l => l.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne<Member>()
            .WithMany()
            .HasForeignKey(l => l.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        // Lending rules count active loans per book and per member
        b.HasIndex(l => new { l.BookId, l.ReturnedAtUtc });
        b.HasIndex(l => new { l.MemberId, l.ReturnedAtUtc });
    }
}
