using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using LibraryApi.Domain.Members;
using Mediator;
using MongoDB.Driver;

namespace LibraryApi.Application.Members.RegisterMember;

public sealed record RegisterMemberCommand(string FullName, string Email) : ICommand<CommandResult<MemberResponse>>;

public sealed class RegisterMemberCommandHandler(IApplicationDbContext dbContext, TimeProvider timeProvider)
    : ICommandHandler<RegisterMemberCommand, CommandResult<MemberResponse>>
{
    public async ValueTask<CommandResult<MemberResponse>> Handle(
        RegisterMemberCommand command,
        CancellationToken cancellationToken)
    {
        var member = Member.Create(command.FullName, command.Email, timeProvider.GetUtcNow().UtcDateTime);

        var emailTaken = await dbContext.Members.Find(m => m.Email == member.Email).AnyAsync(cancellationToken);
        if (emailTaken)
            return CommandResult.Conflict<MemberResponse>("A member with this email already exists");

        await dbContext.Members.InsertOneAsync(member, cancellationToken: cancellationToken);

        return CommandResult.Created(new MemberResponse(member.Id, member.FullName, member.Email));
    }
}
