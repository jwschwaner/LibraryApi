using LibraryApi.Domain.Authors;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Loans;
using LibraryApi.Domain.Members;

namespace LibraryApi.Application.Interfaces;

/// <summary>
/// The read side: query handlers get read-only collections they can only run aggregation
/// pipelines on, so a query can't change state even by accident.
/// </summary>
public interface IReadonlyApplicationDbContext
{
    IReadonlyMongoCollection<Author> Authors { get; }
    IReadonlyMongoCollection<Book> Books { get; }
    IReadonlyMongoCollection<Member> Members { get; }
    IReadonlyMongoCollection<Loan> Loans { get; }
}
