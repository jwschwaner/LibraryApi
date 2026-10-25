using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

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

        var loans = await (
                from l in readonlyDbContext.Loans
                join b in readonlyDbContext.Books on l.BookId equals b.Id
                join m in readonlyDbContext.Members on l.MemberId equals m.Id
                where l.ReturnedAtUtc == null && l.DueOn < today
                orderby l.DueOn
                select new OverdueLoanResponse(l.Id, b.Id, b.Title, m.Id, m.FullName, m.Email, l.DueOn))
            .ToListAsync(cancellationToken);

        return QueryResult.Success<IReadOnlyList<OverdueLoanResponse>>(loans);
    }
}
