using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using MongoDB.Driver.Linq;

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

        var loans = await readonlyDbContext.Loans
            .Where(l => l.ReturnedAtUtc == null && l.DueOn < today)
            .OrderBy(l => l.DueOn)
            .Select(l => new { l.Id, l.BookId, l.MemberId, l.DueOn })
            .ToListAsync(cancellationToken);
        if (loans.Count == 0)
            return QueryResult.Success<IReadOnlyList<OverdueLoanResponse>>([]);

        var bookIds = loans.Select(l => l.BookId).Distinct().ToList();
        var bookTitles = (await readonlyDbContext.Books
                .Where(b => bookIds.Contains(b.Id))
                .Select(b => new { b.Id, b.Title })
                .ToListAsync(cancellationToken))
            .ToDictionary(b => b.Id, b => b.Title);

        var memberIds = loans.Select(l => l.MemberId).Distinct().ToList();
        var members = (await readonlyDbContext.Members
                .Where(m => memberIds.Contains(m.Id))
                .Select(m => new { m.Id, m.FullName, m.Email })
                .ToListAsync(cancellationToken))
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
