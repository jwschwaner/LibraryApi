using LibraryApi.Application.Interfaces;
using LibraryApi.Domain.Books;
using MongoDB.Driver.Linq;

namespace LibraryApi.Application.Books;

public static class BookResponses
{
    /// <summary>
    /// Runs the book query and builds a <see cref="BookResponse"/> per book. Author names and
    /// copies on loan live in other collections, so they come from one query each, not a join.
    /// </summary>
    public static async Task<List<BookResponse>> ToBookResponsesAsync(
        this IQueryable<Book> books,
        IReadonlyApplicationDbContext readonlyDbContext,
        CancellationToken cancellationToken)
    {
        var rows = await books
            .Select(b => new { b.Id, b.Title, b.Isbn, b.PublishedYear, b.AuthorId, b.TotalCopies })
            .ToListAsync(cancellationToken);
        if (rows.Count == 0)
            return [];

        var authorIds = rows.Select(b => b.AuthorId).Distinct().ToList();
        var authorNames = (await readonlyDbContext.Authors
                .Where(a => authorIds.Contains(a.Id))
                .Select(a => new { a.Id, a.FirstName, a.LastName })
                .ToListAsync(cancellationToken))
            .ToDictionary(a => a.Id, a => $"{a.FirstName} {a.LastName}");

        var bookIds = rows.Select(b => b.Id).ToList();
        var copiesOnLoan = (await readonlyDbContext.Loans
                .Where(l => bookIds.Contains(l.BookId) && l.ReturnedAtUtc == null)
                .GroupBy(l => l.BookId)
                .Select(g => new { BookId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken))
            .ToDictionary(x => x.BookId, x => x.Count);

        return rows
            .Select(b => new BookResponse(
                b.Id,
                b.Title,
                b.Isbn,
                b.PublishedYear,
                b.AuthorId,
                authorNames.GetValueOrDefault(b.AuthorId, ""),
                b.TotalCopies,
                b.TotalCopies - copiesOnLoan.GetValueOrDefault(b.Id)))
            .ToList();
    }
}
