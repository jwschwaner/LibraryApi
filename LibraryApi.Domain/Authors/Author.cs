using LibraryApi.Domain.Common;

namespace LibraryApi.Domain.Authors;

public sealed class Author : AuditedEntity
{
    private Author() { }

    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public int? BirthYear { get; private set; }

    public static Author Create(string firstName, string lastName, int? birthYear, DateTime nowUtc)
    {
        var author = new Author
        {
            Id = Guid.CreateVersion7(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            BirthYear = birthYear
        };
        author.MarkCreated(nowUtc);
        return author;
    }

    public void Update(string firstName, string lastName, int? birthYear, DateTime nowUtc)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        BirthYear = birthYear;
        MarkUpdated(nowUtc);
    }
}
