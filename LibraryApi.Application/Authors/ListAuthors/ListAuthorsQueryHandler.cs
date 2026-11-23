using LibraryApi.Application.Common;
using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using MongoDB.Bson;

namespace LibraryApi.Application.Authors.ListAuthors;

public sealed record ListAuthorsQuery : IQuery<QueryResult<IReadOnlyList<AuthorResponse>>>;

public sealed class ListAuthorsQueryHandler(IReadonlyApplicationDbContext readonlyDbContext)
    : IQueryHandler<ListAuthorsQuery, QueryResult<IReadOnlyList<AuthorResponse>>>
{
    public async ValueTask<QueryResult<IReadOnlyList<AuthorResponse>>> Handle(
        ListAuthorsQuery query,
        CancellationToken cancellationToken)
    {
        var authors = await readonlyDbContext.Authors.AggregateAsync(
            [
                new("$sort", new BsonDocument { { "lastName", 1 }, { "firstName", 1 } }),
                new("$project", new BsonDocument { { "firstName", 1 }, { "lastName", 1 }, { "birthYear", 1 } })
            ],
            cancellationToken);

        return QueryResult.Success<IReadOnlyList<AuthorResponse>>(authors
            .Select(x => new AuthorResponse(
                x["_id"].AsGuid,
                x["firstName"].AsString,
                x["lastName"].AsString,
                x["birthYear"].ToNullableInt32()))
            .ToList());
    }
}
