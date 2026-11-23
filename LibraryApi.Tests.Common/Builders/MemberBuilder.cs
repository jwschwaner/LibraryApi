using LibraryApi.Domain.Members;

namespace LibraryApi.Tests.Common.Builders;

/// <summary>
/// Builds members through <see cref="Member.Create"/>. Emails are unique per builder because the
/// collection has a unique index on them.
/// </summary>
public sealed class MemberBuilder
{
    private static int _nextEmail;

    private string _fullName = "Test Member";
    private string _email = $"member{Interlocked.Increment(ref _nextEmail)}@example.com";
    private DateTime _createdAtUtc = TestConstants.NowUtc;

    private MemberBuilder() { }

    public static MemberBuilder Simple() => new();

    public static MemberBuilder Typical() => Simple().WithFullName("Ada Lovelace");

    public MemberBuilder WithFullName(string fullName)
    {
        _fullName = fullName;
        return this;
    }

    public MemberBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public MemberBuilder WithCreatedAtUtc(DateTime createdAtUtc)
    {
        _createdAtUtc = createdAtUtc;
        return this;
    }

    public Member Build() => Member.Create(_fullName, _email, _createdAtUtc);
}
