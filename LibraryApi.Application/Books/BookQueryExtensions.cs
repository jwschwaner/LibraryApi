using LibraryApi.Application.Interfaces;
using LibraryApi.Domain.Books;

namespace LibraryApi.Application.Books;

public static class BookQueryExtensions
{
    /// <summary>
    /// Projects books to <see cref="BookResponse"/>, joining the author's name and counting
    /// active loans in the same SQL query.
    /// </summary>
    public static IQueryable<BookResponse> SelectBookResponse(
        this IQueryable<Book> books,
        IReadonlyApplicationDbContext readonlyDbContext)
    {
        return books.Select(b => new BookResponse(
            b.Id,
            b.Title,
            b.Isbn,
            b.PublishedYear,
            b.AuthorId,
            readonlyDbContext.Authors
                .Where(a => a.Id == b.AuthorId)
                .Select(a => a.FirstName + " " + a.LastName)
                .First(),
            b.TotalCopies,
            b.TotalCopies - readonlyDbContext.Loans.Count(l => l.BookId == b.Id && l.ReturnedAtUtc == null)));
    }
}
