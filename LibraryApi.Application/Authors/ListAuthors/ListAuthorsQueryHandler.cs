using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using LibraryApi.Domain.Authors;
using Mediator;
using MongoDB.Driver;

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
            new EmptyPipelineDefinition<Author>()
                .Sort(Builders<Author>.Sort.Ascending(a => a.LastName).Ascending(a => a.FirstName))
                .Project(a => new AuthorResponse(a.Id, a.FirstName, a.LastName, a.BirthYear)),
            cancellationToken);

        return QueryResult.Success<IReadOnlyList<AuthorResponse>>(authors);
    }
}
