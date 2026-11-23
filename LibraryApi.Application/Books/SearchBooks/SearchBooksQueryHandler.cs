using System.Text.RegularExpressions;
using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using MongoDB.Bson;

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
        var filter = new BsonDocument();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Case-insensitive $regex; escaped so user input is matched literally
            var pattern = new BsonRegularExpression(Regex.Escape(query.Search.Trim()), "i");

            var authors = await readonlyDbContext.Authors.AggregateAsync(
                [
                    new("$match", new BsonDocument("$or", new BsonArray
                    {
                        new BsonDocument("firstName", pattern),
                        new BsonDocument("lastName", pattern)
                    })),
                    new("$project", new BsonDocument("_id", 1))
                ],
                cancellationToken);

            filter.Add("$or", new BsonArray
            {
                new BsonDocument("title", pattern),
                new BsonDocument("authorId", new BsonDocument("$in", new BsonArray(authors.Select(x => x["_id"]))))
            });
        }

        var result = await readonlyDbContext.GetBookResponsesAsync(
            filter, new BsonDocument("title", 1), cancellationToken);

        // Availability is derived from the loans collection, so it can only be filtered here
        if (query.AvailableOnly)
            result = result.Where(b => b.AvailableCopies > 0).ToList();

        return QueryResult.Success<IReadOnlyList<BookResponse>>(result);
    }
}
