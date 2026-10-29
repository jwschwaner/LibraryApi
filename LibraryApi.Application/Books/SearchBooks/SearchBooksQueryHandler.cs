using System.Text.RegularExpressions;
using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using LibraryApi.Domain.Books;
using Mediator;
using MongoDB.Driver.Linq;

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
        IQueryable<Book> books = readonlyDbContext.Books;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Translated to a case-insensitive $regex; escaped so user input is matched literally
            var pattern = new Regex(Regex.Escape(query.Search.Trim()), RegexOptions.IgnoreCase);

            var authorIds = await readonlyDbContext.Authors
                .Where(a => pattern.IsMatch(a.FirstName) || pattern.IsMatch(a.LastName))
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);

            books = books.Where(b => pattern.IsMatch(b.Title) || authorIds.Contains(b.AuthorId));
        }

        var result = await books
            .OrderBy(b => b.Title)
            .ToBookResponsesAsync(readonlyDbContext, cancellationToken);

        // Availability is derived from the loans collection, so it can only be filtered here
        if (query.AvailableOnly)
            result = result.Where(b => b.AvailableCopies > 0).ToList();

        return QueryResult.Success<IReadOnlyList<BookResponse>>(result);
    }
}
