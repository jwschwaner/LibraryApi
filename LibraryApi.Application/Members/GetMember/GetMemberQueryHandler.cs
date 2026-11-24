using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Loans;
using LibraryApi.Domain.Members;
using Mediator;
using MongoDB.Driver;

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
        var members = await readonlyDbContext.Members.AggregateAsync(
            new EmptyPipelineDefinition<Member>()
                .Match(Builders<Member>.Filter.Eq(m => m.Id, query.Id))
                .Project(m => new { m.Id, m.FullName, m.Email }),
            cancellationToken);
        var member = members.SingleOrDefault();
        if (member is null)
            return QueryResult.NotFound<MemberDetailsResponse>("Member");

        var loans = await readonlyDbContext.Loans.AggregateAsync(
            new EmptyPipelineDefinition<Loan>()
                .Match(Builders<Loan>.Filter.Eq(l => l.MemberId, query.Id)
                       & Builders<Loan>.Filter.Eq(l => l.ReturnedAtUtc, null))
                .Sort(Builders<Loan>.Sort.Ascending(l => l.DueOn))
                .Project(l => new { l.Id, l.BookId, l.DueOn }),
            cancellationToken);

        var bookIds = loans.Select(l => l.BookId).Distinct();
        var books = await readonlyDbContext.Books.AggregateAsync(
            new EmptyPipelineDefinition<Book>()
                .Match(Builders<Book>.Filter.In(b => b.Id, bookIds))
                .Project(b => new { b.Id, b.Title }),
            cancellationToken);
        var bookTitles = books.ToDictionary(b => b.Id, b => b.Title);

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
