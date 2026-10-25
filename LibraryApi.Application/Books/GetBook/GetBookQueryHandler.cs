using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Application.Books.GetBook;

public sealed record GetBookQuery(Guid Id) : IQuery<QueryResult<BookResponse>>;

public sealed class GetBookQueryHandler(IReadonlyApplicationDbContext readonlyDbContext)
    : IQueryHandler<GetBookQuery, QueryResult<BookResponse>>
{
    public async ValueTask<QueryResult<BookResponse>> Handle(GetBookQuery query, CancellationToken cancellationToken)
    {
        var book = await readonlyDbContext.Books
            .Where(b => b.Id == query.Id)
            .SelectBookResponse(readonlyDbContext)
            .SingleOrDefaultAsync(cancellationToken);

        return QueryResult.FromNullable(book, "Book");
    }
}
