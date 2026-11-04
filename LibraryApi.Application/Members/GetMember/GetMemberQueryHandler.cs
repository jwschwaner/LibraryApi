using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using MongoDB.Driver.Linq;

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
        var member = await readonlyDbContext.Members
            .Where(m => m.Id == query.Id)
            .Select(m => new { m.Id, m.FullName, m.Email })
            .SingleOrDefaultAsync(cancellationToken);
        if (member is null)
            return QueryResult.NotFound<MemberDetailsResponse>("Member");

        var loans = await readonlyDbContext.Loans
            .Where(l => l.MemberId == query.Id && l.ReturnedAtUtc == null)
            .OrderBy(l => l.DueOn)
            .Select(l => new { l.Id, l.BookId, l.DueOn })
            .ToListAsync(cancellationToken);

        var bookIds = loans.Select(l => l.BookId).Distinct().ToList();
        var bookTitles = (await readonlyDbContext.Books
                .Where(b => bookIds.Contains(b.Id))
                .Select(b => new { b.Id, b.Title })
                .ToListAsync(cancellationToken))
            .ToDictionary(b => b.Id, b => b.Title);

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        return QueryResult.Success(new MemberDetailsResponse(
            member.Id,
            member.FullName,
            member.Email,
            loans
                .Select(l => new MemberLoanResponse(
                    l.Id, l.BookId, bookTitles.GetValueOrDefault(l.BookId, ""), l.DueOn, l.DueOn < today))
                .ToList()));
    }
}
