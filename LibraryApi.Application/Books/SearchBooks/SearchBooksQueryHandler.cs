using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Application.Books.SearchBooks;

/// <param name="Search">Matches title or author name, case-insensitively. Null lists everything.</param>
/// <param name="AvailableOnly">Only books with at least one copy on the shelf.</param>
public sealed record SearchBooksQuery(string? Search, bool AvailableOnly)
    : IQuery<QueryResult<IReadOnlyList<BookResponse>>>;

public sealed class SearchBooksQueryHandler(IReadonlyApplicationDbContext readonlyDbContext)
    : IQueryHandler<SearchBooksQuery, QueryResult<IReadOnlyList<BookResponse>>>
{
    public async ValueTask<QueryResult<IReadOnlyList<BookResponse>>> Handle(
        SearchBooksQuery query,
        CancellationToken cancellationToken)
    {
        // Filter on the entities and project last: EF can't translate filters on the response record
        var books = readonlyDbContext.Books;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            books = books.Where(b =>
                b.Title.ToLower().Contains(term) ||
                readonlyDbContext.Authors.Any(a =>
                    a.Id == b.AuthorId && (a.FirstName + " " + a.LastName).ToLower().Contains(term)));
        }

        if (query.AvailableOnly)
            books = books.Where(b =>
                readonlyDbContext.Loans.Count(l => l.BookId == b.Id && l.ReturnedAtUtc == null) < b.TotalCopies);

        var result = await books
            .OrderBy(b => b.Title)
            .SelectBookResponse(readonlyDbContext)
            .ToListAsync(cancellationToken);

        return QueryResult.Success<IReadOnlyList<BookResponse>>(result);
    }
}
