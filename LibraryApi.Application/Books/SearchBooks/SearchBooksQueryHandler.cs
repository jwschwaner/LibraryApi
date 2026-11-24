using System.Text.RegularExpressions;
using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using LibraryApi.Domain.Authors;
using LibraryApi.Domain.Books;
using Mediator;
using MongoDB.Bson;
using MongoDB.Driver;

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
        var filter = Builders<Book>.Filter.Empty;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Case-insensitive $regex; escaped so user input is matched literally
            var pattern = new BsonRegularExpression(Regex.Escape(query.Search.Trim()), "i");

            var authors = await readonlyDbContext.Authors.AggregateAsync(
                new EmptyPipelineDefinition<Author>()
                    .Match(Builders<Author>.Filter.Regex(a => a.FirstName, pattern)
                           | Builders<Author>.Filter.Regex(a => a.LastName, pattern))
                    .Project(a => new { a.Id }),
                cancellationToken);

            filter = Builders<Book>.Filter.Regex(b => b.Title, pattern)
                     | Builders<Book>.Filter.In(b => b.AuthorId, authors.Select(a => a.Id));
        }

        var result = await readonlyDbContext.GetBookResponsesAsync(
            filter, Builders<Book>.Sort.Ascending(b => b.Title), cancellationToken);

        // Availability is derived from the loans collection, so it can only be filtered here
        if (query.AvailableOnly)
            result = result.Where(b => b.AvailableCopies > 0).ToList();

        return QueryResult.Success<IReadOnlyList<BookResponse>>(result);
    }
}
