using LibraryApi.Application.Common;
using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using MongoDB.Bson;

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
            [
                new("$match", new BsonDocument("_id", query.Id.ToBson())),
                new("$project", new BsonDocument { { "fullName", 1 }, { "email", 1 } })
            ],
            cancellationToken);
        var member = members.SingleOrDefault();
        if (member is null)
            return QueryResult.NotFound<MemberDetailsResponse>("Member");

        var loans = await readonlyDbContext.Loans.AggregateAsync(
            [
                new("$match", new BsonDocument
                {
                    { "memberId", query.Id.ToBson() },
                    { "returnedAtUtc", BsonNull.Value }
                }),
                new("$sort", new BsonDocument("dueOn", 1)),
                new("$project", new BsonDocument { { "bookId", 1 }, { "dueOn", 1 } })
            ],
            cancellationToken);

        var bookIds = loans.Select(x => x["bookId"].AsGuid).Distinct();
        var books = await readonlyDbContext.Books.AggregateAsync(
            [
                new("$match", new BsonDocument("_id", new BsonDocument("$in", bookIds.ToBson()))),
                new("$project", new BsonDocument("title", 1))
            ],
            cancellationToken);
        var bookTitles = books.ToDictionary(x => x["_id"].AsGuid, x => x["title"].AsString);

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        return QueryResult.Success(new MemberDetailsResponse(
            member["_id"].AsGuid,
            member["fullName"].AsString,
            member["email"].AsString,
            loans
                .Select(x =>
                {
                    var bookId = x["bookId"].AsGuid;
                    var dueOn = x["dueOn"].ToDateOnly();
                    return new MemberLoanResponse(
                        x["_id"].AsGuid, bookId, bookTitles.GetValueOrDefault(bookId, ""), dueOn, dueOn < today);
                })
                .ToList()));
    }
}
