using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Loans;
using LibraryApi.Domain.Members;
using Mediator;
using MongoDB.Driver;

namespace LibraryApi.Application.Loans.OverdueLoans;

public sealed record OverdueLoansQuery : IQuery<QueryResult<IReadOnlyList<OverdueLoanResponse>>>;

public sealed record OverdueLoanResponse(
    Guid LoanId,
    Guid BookId,
    string BookTitle,
    Guid MemberId,
    string MemberName,
    string MemberEmail,
    DateOnly DueOn
);

public sealed class OverdueLoansQueryHandler(IReadonlyApplicationDbContext readonlyDbContext, TimeProvider timeProvider)
    : IQueryHandler<OverdueLoansQuery, QueryResult<IReadOnlyList<OverdueLoanResponse>>>
{
    public async ValueTask<QueryResult<IReadOnlyList<OverdueLoanResponse>>> Handle(
        OverdueLoansQuery query,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        var loans = await readonlyDbContext.Loans.AggregateAsync(
            new EmptyPipelineDefinition<Loan>()
                .Match(Builders<Loan>.Filter.Eq(l => l.ReturnedAtUtc, null)
                       & Builders<Loan>.Filter.Lt(l => l.DueOn, today))
                .Sort(Builders<Loan>.Sort.Ascending(l => l.DueOn))
                .Project(l => new { l.Id, l.BookId, l.MemberId, l.DueOn }),
            cancellationToken);
        if (loans.Count == 0)
            return QueryResult.Success<IReadOnlyList<OverdueLoanResponse>>([]);

        var bookIds = loans.Select(l => l.BookId).Distinct();
        var books = await readonlyDbContext.Books.AggregateAsync(
            new EmptyPipelineDefinition<Book>()
                .Match(Builders<Book>.Filter.In(b => b.Id, bookIds))
                .Project(b => new { b.Id, b.Title }),
            cancellationToken);
        var bookTitles = books.ToDictionary(b => b.Id, b => b.Title);

        var memberIds = loans.Select(l => l.MemberId).Distinct();
        var members = (await readonlyDbContext.Members.AggregateAsync(
                new EmptyPipelineDefinition<Member>()
                    .Match(Builders<Member>.Filter.In(m => m.Id, memberIds))
                    .Project(m => new { m.Id, m.FullName, m.Email }),
                cancellationToken))
            .ToDictionary(m => m.Id);

        var result = loans
            .Select(l =>
            {
                var member = members.GetValueOrDefault(l.MemberId);
                return new OverdueLoanResponse(
                    l.Id,
                    l.BookId,
                    bookTitles.GetValueOrDefault(l.BookId, ""),
                    l.MemberId,
                    member?.FullName ?? "",
                    member?.Email ?? "",
                    l.DueOn);
            })
            .ToList();

        return QueryResult.Success<IReadOnlyList<OverdueLoanResponse>>(result);
    }
}
