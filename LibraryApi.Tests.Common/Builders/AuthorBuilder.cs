using LibraryApi.Domain.Authors;

namespace LibraryApi.Tests.Common.Builders;

/// <summary>
/// Builds authors through <see cref="Author.Create"/>, so test data passes the same rules as real
/// data. <see cref="Simple"/> is the least that's valid, <see cref="Typical"/> is fully populated.
/// </summary>
public sealed class AuthorBuilder
{
    private string _firstName = "Test";
    private string _lastName = "Author";
    private int? _birthYear;
    private DateTime _createdAtUtc = TestConstants.NowUtc;

    private AuthorBuilder() { }

    public static AuthorBuilder Simple() => new();

    public static AuthorBuilder Typical() => Simple()
        .WithFirstName("Ursula K.")
        .WithLastName("Le Guin")
        .WithBirthYear(1929);

    public AuthorBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public AuthorBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public AuthorBuilder WithBirthYear(int? birthYear)
    {
        _birthYear = birthYear;
        return this;
    }

    public AuthorBuilder WithCreatedAtUtc(DateTime createdAtUtc)
    {
        _createdAtUtc = createdAtUtc;
        return this;
    }

    public Author Build() => Author.Create(_firstName, _lastName, _birthYear, _createdAtUtc);
}
