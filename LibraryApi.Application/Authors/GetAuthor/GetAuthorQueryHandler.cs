using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

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
        // Project straight to the response: the read side never materialises domain entities
        var author = await readonlyDbContext.Authors
            .Where(a => a.Id == query.Id)
            .Select(a => new AuthorDetailsResponse(
                a.Id,
                a.FirstName,
                a.LastName,
                a.BirthYear,
                a.Books
                    .OrderBy(b => b.PublishedYear)
                    .Select(b => new AuthorBookResponse(b.Id, b.Title, b.PublishedYear))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        return QueryResult.FromNullable(author, "Author");
    }
}
