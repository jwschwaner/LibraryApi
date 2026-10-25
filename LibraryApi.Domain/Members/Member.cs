using LibraryApi.Domain.Common;

namespace LibraryApi.Domain.Members;

public sealed class Member : AuditedEntity
{
    private Member() { }

    public string FullName { get; private set; } = null!;
    public string Email { get; private set; } = null!;

    public static Member Create(string fullName, string email)
    {
        return new Member
        {
            Id = Guid.CreateVersion7(),
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant()
        };
    }
}
