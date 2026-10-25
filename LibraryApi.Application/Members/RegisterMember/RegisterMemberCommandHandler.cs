using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using LibraryApi.Domain.Members;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Application.Members.RegisterMember;

public sealed record RegisterMemberCommand(string FullName, string Email) : ICommand<CommandResult<MemberResponse>>;

public sealed class RegisterMemberCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<RegisterMemberCommand, CommandResult<MemberResponse>>
{
    public async ValueTask<CommandResult<MemberResponse>> Handle(
        RegisterMemberCommand command,
        CancellationToken cancellationToken)
    {
        var member = Member.Create(command.FullName, command.Email);

        var emailTaken = await dbContext.Members.AnyAsync(m => m.Email == member.Email, cancellationToken);
        if (emailTaken)
            return CommandResult.Conflict<MemberResponse>("A member with this email already exists");

        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CommandResult.Created(new MemberResponse(member.Id, member.FullName, member.Email));
    }
}
