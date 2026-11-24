using LibraryApi.Application.Interfaces;
using LibraryApi.Domain.Authors;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Loans;
using MongoDB.Driver;

namespace LibraryApi.Application.Books;

public static class BookResponses
{
    /// <summary>
    /// Finds the books matching <paramref name="filter"/> and builds a <see cref="BookResponse"/>
    /// per book. Author names and copies on loan live in other collections, so they come from one
    /// query each.
    /// </summary>
    public static async Task<List<BookResponse>> GetBookResponsesAsync(
        this IReadonlyApplicationDbContext readonlyDbContext,
        FilterDefinition<Book> filter,
        SortDefinition<Book>? sort,
        CancellationToken cancellationToken)
    {
        PipelineDefinition<Book, Book> pipeline = new EmptyPipelineDefinition<Book>().Match(filter);
        if (sort is not null)
            pipeline = pipeline.Sort(sort);

        var books = await readonlyDbContext.Books.AggregateAsync(
            pipeline.Project(b => new { b.Id, b.Title, b.Isbn, b.PublishedYear, b.AuthorId, b.TotalCopies }),
            cancellationToken);
        if (books.Count == 0)
            return [];

        var authorIds = books.Select(b => b.AuthorId).Distinct();
        var authors = await readonlyDbContext.Authors.AggregateAsync(
            new EmptyPipelineDefinition<Author>()
                .Match(Builders<Author>.Filter.In(a => a.Id, authorIds))
                .Project(a => new { a.Id, a.FirstName, a.LastName }),
            cancellationToken);
        var authorNames = authors.ToDictionary(a => a.Id, a => $"{a.FirstName} {a.LastName}");

        var bookIds = books.Select(b => b.Id);
        var loanCounts = await readonlyDbContext.Loans.AggregateAsync(
            new EmptyPipelineDefinition<Loan>()
                .Match(Builders<Loan>.Filter.In(l => l.BookId, bookIds)
                       & Builders<Loan>.Filter.Eq(l => l.ReturnedAtUtc, null))
                .Group(l => l.BookId, g => new { BookId = g.Key, Count = g.Count() }),
            cancellationToken);
        var copiesOnLoan = loanCounts.ToDictionary(x => x.BookId, x => x.Count);

        return books
            .Select(b => new BookResponse(
                b.Id,
                b.Title,
                b.Isbn,
                b.PublishedYear,
                b.AuthorId,
                authorNames.GetValueOrDefault(b.AuthorId, ""),
                b.TotalCopies,
                b.TotalCopies - copiesOnLoan.GetValueOrDefault(b.Id)))
            .ToList();
    }
}
