using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using MongoDB.Driver.Linq;

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
        var author = await readonlyDbContext.Authors
            .Where(a => a.Id == query.Id)
            .Select(a => new { a.Id, a.FirstName, a.LastName, a.BirthYear })
            .SingleOrDefaultAsync(cancellationToken);
        if (author is null)
            return QueryResult.NotFound<AuthorDetailsResponse>("Author");

        var books = await readonlyDbContext.Books
            .Where(b => b.AuthorId == query.Id)
            .OrderBy(b => b.PublishedYear)
            .Select(b => new AuthorBookResponse(b.Id, b.Title, b.PublishedYear))
            .ToListAsync(cancellationToken);

        return QueryResult.Success(new AuthorDetailsResponse(
            author.Id, author.FirstName, author.LastName, author.BirthYear, books));
    }
}
