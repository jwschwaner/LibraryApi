using LibraryApi.Application.Common;
using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using MongoDB.Bson;

namespace LibraryApi.Application.Authors.GetAuthor;

public sealed record GetAuthorQuery(Guid Id) : IQuery<QueryResult<AuthorDetailsResponse>>;

public sealed record AuthorDetailsResponse(
    Guid Id,
    string FirstName,
    string LastName,
    int? BirthYear,
    IReadOnlyList<AuthorBookResponse> Books
);

public sealed record AuthorBookResponse(Guid Id, string Title, int PublishedYear);

public sealed class GetAuthorQueryHandler(IReadonlyApplicationDbContext readonlyDbContext)
    : IQueryHandler<GetAuthorQuery, QueryResult<AuthorDetailsResponse>>
{
    public async ValueTask<QueryResult<AuthorDetailsResponse>> Handle(
        GetAuthorQuery query,
        CancellationToken cancellationToken)
    {
        // Project in the database: the read side never materialises domain entities
        var authors = await readonlyDbContext.Authors.AggregateAsync(
            [
                new("$match", new BsonDocument("_id", query.Id.ToBson())),
                new("$project", new BsonDocument { { "firstName", 1 }, { "lastName", 1 }, { "birthYear", 1 } })
            ],
            cancellationToken);
        var author = authors.SingleOrDefault();
        if (author is null)
            return QueryResult.NotFound<AuthorDetailsResponse>("Author");

        var books = await readonlyDbContext.Books.AggregateAsync(
            [
                new("$match", new BsonDocument("authorId", query.Id.ToBson())),
                new("$sort", new BsonDocument("publishedYear", 1)),
                new("$project", new BsonDocument { { "title", 1 }, { "publishedYear", 1 } })
            ],
            cancellationToken);

        return QueryResult.Success(new AuthorDetailsResponse(
            author["_id"].AsGuid,
            author["firstName"].AsString,
            author["lastName"].AsString,
            author["birthYear"].ToNullableInt32(),
            books
                .Select(x => new AuthorBookResponse(x["_id"].AsGuid, x["title"].AsString, x["publishedYear"].ToInt32()))
                .ToList()));
    }
}
