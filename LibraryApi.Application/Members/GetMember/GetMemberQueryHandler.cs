using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Application.Members.GetMember;

public sealed record GetMemberQuery(Guid Id) : IQuery<QueryResult<MemberDetailsResponse>>;

public sealed record MemberDetailsResponse(
    Guid Id,
    string FullName,
    string Email,
    IReadOnlyList<MemberLoanResponse> ActiveLoans
);

public sealed record MemberLoanResponse(Guid LoanId, Guid BookId, string BookTitle, DateOnly DueOn, bool IsOverdue);

public sealed class GetMemberQueryHandler(IReadonlyApplicationDbContext readonlyDbContext, TimeProvider timeProvider)
    : IQueryHandler<GetMemberQuery, QueryResult<MemberDetailsResponse>>
{
    public async ValueTask<QueryResult<MemberDetailsResponse>> Handle(
        GetMemberQuery query,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        var member = await readonlyDbContext.Members
            .Where(m => m.Id == query.Id)
            .Select(m => new MemberDetailsResponse(
                m.Id,
                m.FullName,
                m.Email,
                readonlyDbContext.Loans
                    .Where(l => l.MemberId == m.Id && l.ReturnedAtUtc == null)
                    .OrderBy(l => l.DueOn)
                    .Join(readonlyDbContext.Books, l => l.BookId, b => b.Id,
                        (l, b) => new MemberLoanResponse(l.Id, b.Id, b.Title, l.DueOn, l.DueOn < today))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        return QueryResult.FromNullable(member, "Member");
    }
}
