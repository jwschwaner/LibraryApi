using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using LibraryApi.Domain.Books;
using Mediator;
using MongoDB.Driver;

namespace LibraryApi.Application.Books.GetBook;

public sealed record GetBookQuery(Guid Id) : IQuery<QueryResult<BookResponse>>;

public sealed class GetBookQueryHandler(IReadonlyApplicationDbContext readonlyDbContext)
    : IQueryHandler<GetBookQuery, QueryResult<BookResponse>>
{
    public async ValueTask<QueryResult<BookResponse>> Handle(GetBookQuery query, CancellationToken cancellationToken)
    {
        var books = await readonlyDbContext.GetBookResponsesAsync(
            Builders<Book>.Filter.Eq(b => b.Id, query.Id), sort: null, cancellationToken);

        return QueryResult.FromNullable(books.SingleOrDefault(), "Book");
    }
}
