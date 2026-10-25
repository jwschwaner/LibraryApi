using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Application.Members.ListMembers;

public sealed record ListMembersQuery : IQuery<QueryResult<IReadOnlyList<MemberResponse>>>;

public sealed class ListMembersQueryHandler(IReadonlyApplicationDbContext readonlyDbContext)
    : IQueryHandler<ListMembersQuery, QueryResult<IReadOnlyList<MemberResponse>>>
{
    public async ValueTask<QueryResult<IReadOnlyList<MemberResponse>>> Handle(
        ListMembersQuery query,
        CancellationToken cancellationToken)
    {
        var members = await readonlyDbContext.Members
            .OrderBy(m => m.FullName)
            .Select(m => new MemberResponse(m.Id, m.FullName, m.Email))
            .ToListAsync(cancellationToken);

        return QueryResult.Success<IReadOnlyList<MemberResponse>>(members);
    }
}
