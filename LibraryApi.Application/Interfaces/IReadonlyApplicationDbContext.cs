using LibraryApi.Domain.Authors;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Loans;
using LibraryApi.Domain.Members;

namespace LibraryApi.Application.Interfaces;

/// <summary>
/// The read side: query handlers get untracked IQueryables and no way to save, so a query
/// can't change state even by accident.
/// </summary>
public interface IReadonlyApplicationDbContext
{
    IQueryable<Author> Authors { get; }
    IQueryable<Book> Books { get; }
    IQueryable<Member> Members { get; }
    IQueryable<Loan> Loans { get; }
}
