using LibraryApi.Application.Common;
using LibraryApi.Application.Interfaces;
using MongoDB.Bson;

namespace LibraryApi.Application.Books;

public static class BookResponses
{
    /// <summary>
    /// Finds the books matching <paramref name="filter"/> and builds a <see cref="BookResponse"/>
    /// per book. Author names and copies on loan live in other collections, so they come from one
    /// query each, not a join.
    /// </summary>
    public static async Task<List<BookResponse>> GetBookResponsesAsync(
        this IReadonlyApplicationDbContext readonlyDbContext,
        BsonDocument filter,
        BsonDocument? sort,
        CancellationToken cancellationToken)
    {
        var pipeline = new List<BsonDocument> { new("$match", filter) };
        if (sort is not null)
            pipeline.Add(new BsonDocument("$sort", sort));
        pipeline.Add(new BsonDocument("$project", new BsonDocument
        {
            { "title", 1 }, { "isbn", 1 }, { "publishedYear", 1 }, { "authorId", 1 }, { "totalCopies", 1 }
        }));

        var books = await readonlyDbContext.Books.AggregateAsync(pipeline, cancellationToken);
        if (books.Count == 0)
            return [];

        var authorIds = books.Select(x => x["authorId"].AsGuid).Distinct();
        var authors = await readonlyDbContext.Authors.AggregateAsync(
            [
                new("$match", new BsonDocument("_id", new BsonDocument("$in", authorIds.ToBson()))),
                new("$project", new BsonDocument { { "firstName", 1 }, { "lastName", 1 } })
            ],
            cancellationToken);
        var authorNames = authors.ToDictionary(
            x => x["_id"].AsGuid,
            x => $"{x["firstName"].AsString} {x["lastName"].AsString}");

        var bookIds = books.Select(x => x["_id"].AsGuid);
        var loanCounts = await readonlyDbContext.Loans.AggregateAsync(
            [
                new("$match", new BsonDocument
                {
                    { "bookId", new BsonDocument("$in", bookIds.ToBson()) },
                    { "returnedAtUtc", BsonNull.Value }
                }),
                new("$group", new BsonDocument { { "_id", "$bookId" }, { "n", new BsonDocument("$sum", 1) } })
            ],
            cancellationToken);
        var copiesOnLoan = loanCounts.ToDictionary(x => x["_id"].AsGuid, x => x["n"].ToInt32());

        return books
            .Select(x =>
            {
                var id = x["_id"].AsGuid;
                var authorId = x["authorId"].AsGuid;
                var totalCopies = x["totalCopies"].ToInt32();
                return new BookResponse(
                    id,
                    x["title"].AsString,
                    x["isbn"].AsString,
                    x["publishedYear"].ToInt32(),
                    authorId,
                    authorNames.GetValueOrDefault(authorId, ""),
                    totalCopies,
                    totalCopies - copiesOnLoan.GetValueOrDefault(id));
            })
            .ToList();
    }
}
