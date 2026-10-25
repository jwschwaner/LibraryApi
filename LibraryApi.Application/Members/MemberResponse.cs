namespace LibraryApi.Application.Members;

public sealed record MemberResponse(Guid Id, string FullName, string Email);
