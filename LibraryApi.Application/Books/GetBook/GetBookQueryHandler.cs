using LibraryApi.Application.Common;
using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using MongoDB.Bson;

namespace LibraryApi.Application.Books.GetBook;

public sealed record GetBookQuery(Guid Id) : IQuery<QueryResult<BookResponse>>;

public sealed class GetBookQueryHandler(IReadonlyApplicationDbContext readonlyDbContext)
    : IQueryHandler<GetBookQuery, QueryResult<BookResponse>>
{
    public async ValueTask<QueryResult<BookResponse>> Handle(GetBookQuery query, CancellationToken cancellationToken)
    {
        var books = await readonlyDbContext.GetBookResponsesAsync(
            new BsonDocument("_id", query.Id.ToBson()), sort: null, cancellationToken);

        return QueryResult.FromNullable(books.SingleOrDefault(), "Book");
    }
}
