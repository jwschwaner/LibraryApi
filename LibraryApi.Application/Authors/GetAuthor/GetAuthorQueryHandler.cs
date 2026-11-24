using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using LibraryApi.Domain.Authors;
using LibraryApi.Domain.Books;
using Mediator;
using MongoDB.Driver;

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
            new EmptyPipelineDefinition<Author>()
                .Match(Builders<Author>.Filter.Eq(a => a.Id, query.Id))
                .Project(a => new { a.Id, a.FirstName, a.LastName, a.BirthYear }),
            cancellationToken);
        var author = authors.SingleOrDefault();
        if (author is null)
            return QueryResult.NotFound<AuthorDetailsResponse>("Author");

        var books = await readonlyDbContext.Books.AggregateAsync(
            new EmptyPipelineDefinition<Book>()
                .Match(Builders<Book>.Filter.Eq(b => b.AuthorId, query.Id))
                .Sort(Builders<Book>.Sort.Ascending(b => b.PublishedYear))
                .Project(b => new AuthorBookResponse(b.Id, b.Title, b.PublishedYear)),
            cancellationToken);

        return QueryResult.Success(new AuthorDetailsResponse(
            author.Id, author.FirstName, author.LastName, author.BirthYear, books));
    }
}
