using LibraryApi.Domain.Authors;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Loans;
using LibraryApi.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Application.Interfaces;

/// <summary>The write side: command handlers load tracked entities, change them and save.</summary>
public interface IApplicationDbContext
{
    DbSet<Author> Authors { get; }
    DbSet<Book> Books { get; }
    DbSet<Member> Members { get; }
    DbSet<Loan> Loans { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
