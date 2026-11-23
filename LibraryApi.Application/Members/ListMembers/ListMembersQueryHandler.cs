using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using MongoDB.Bson;

namespace LibraryApi.Application.Members.ListMembers;

public sealed record ListMembersQuery : IQuery<QueryResult<IReadOnlyList<MemberResponse>>>;

public sealed class ListMembersQueryHandler(IReadonlyApplicationDbContext readonlyDbContext)
    : IQueryHandler<ListMembersQuery, QueryResult<IReadOnlyList<MemberResponse>>>
{
    public async ValueTask<QueryResult<IReadOnlyList<MemberResponse>>> Handle(
        ListMembersQuery query,
        CancellationToken cancellationToken)
    {
        var members = await readonlyDbContext.Members.AggregateAsync(
            [
                new("$sort", new BsonDocument("fullName", 1)),
                new("$project", new BsonDocument { { "fullName", 1 }, { "email", 1 } })
            ],
            cancellationToken);

        return QueryResult.Success<IReadOnlyList<MemberResponse>>(members
            .Select(x => new MemberResponse(x["_id"].AsGuid, x["fullName"].AsString, x["email"].AsString))
            .ToList());
    }
}
