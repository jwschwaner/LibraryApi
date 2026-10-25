using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Application.Authors.ListAuthors;

public sealed record ListAuthorsQuery : IQuery<QueryResult<IReadOnlyList<AuthorResponse>>>;

public sealed class ListAuthorsQueryHandler(IReadonlyApplicationDbContext readonlyDbContext)
    : IQueryHandler<ListAuthorsQuery, QueryResult<IReadOnlyList<AuthorResponse>>>
{
    public async ValueTask<QueryResult<IReadOnlyList<AuthorResponse>>> Handle(
        ListAuthorsQuery query,
        CancellationToken cancellationToken)
    {
        var authors = await readonlyDbContext.Authors
            .OrderBy(a => a.LastName)
            .ThenBy(a => a.FirstName)
            .Select(a => new AuthorResponse(a.Id, a.FirstName, a.LastName, a.BirthYear))
            .ToListAsync(cancellationToken);

        return QueryResult.Success<IReadOnlyList<AuthorResponse>>(authors);
    }
}
