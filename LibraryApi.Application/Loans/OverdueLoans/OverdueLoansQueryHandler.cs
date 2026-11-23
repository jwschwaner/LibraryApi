using LibraryApi.Application.Common;
using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using MongoDB.Bson;

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
            [
                new("$match", new BsonDocument
                {
                    { "returnedAtUtc", BsonNull.Value },
                    { "dueOn", new BsonDocument("$lt", today.ToBson()) }
                }),
                new("$sort", new BsonDocument("dueOn", 1)),
                new("$project", new BsonDocument { { "bookId", 1 }, { "memberId", 1 }, { "dueOn", 1 } })
            ],
            cancellationToken);
        if (loans.Count == 0)
            return QueryResult.Success<IReadOnlyList<OverdueLoanResponse>>([]);

        var bookIds = loans.Select(x => x["bookId"].AsGuid).Distinct();
        var books = await readonlyDbContext.Books.AggregateAsync(
            [
                new("$match", new BsonDocument("_id", new BsonDocument("$in", bookIds.ToBson()))),
                new("$project", new BsonDocument("title", 1))
            ],
            cancellationToken);
        var bookTitles = books.ToDictionary(x => x["_id"].AsGuid, x => x["title"].AsString);

        var memberIds = loans.Select(x => x["memberId"].AsGuid).Distinct();
        var members = (await readonlyDbContext.Members.AggregateAsync(
                [
                    new("$match", new BsonDocument("_id", new BsonDocument("$in", memberIds.ToBson()))),
                    new("$project", new BsonDocument { { "fullName", 1 }, { "email", 1 } })
                ],
                cancellationToken))
            .ToDictionary(x => x["_id"].AsGuid);

        var result = loans
            .Select(x =>
            {
                var bookId = x["bookId"].AsGuid;
                var memberId = x["memberId"].AsGuid;
                var member = members.GetValueOrDefault(memberId);
                return new OverdueLoanResponse(
                    x["_id"].AsGuid,
                    bookId,
                    bookTitles.GetValueOrDefault(bookId, ""),
                    memberId,
                    member?["fullName"].AsString ?? "",
                    member?["email"].AsString ?? "",
                    x["dueOn"].ToDateOnly());
            })
            .ToList();

        return QueryResult.Success<IReadOnlyList<OverdueLoanResponse>>(result);
    }
}
