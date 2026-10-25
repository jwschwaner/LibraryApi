using LibraryApi.Application.Members.RegisterMember;

namespace LibraryApi.Api.Controllers.Members.Requests;

public sealed record RegisterMemberRequest(string FullName, string Email)
{
    public RegisterMemberCommand ToCommand() => new(FullName, Email);
}
