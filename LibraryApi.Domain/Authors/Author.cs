using LibraryApi.Domain.Books;
using LibraryApi.Domain.Common;

namespace LibraryApi.Domain.Authors;

public sealed class Author : AuditedEntity
{
    private readonly List<Book> _books = [];

    private Author() { }

    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public int? BirthYear { get; private set; }

    public IReadOnlyCollection<Book> Books => _books.AsReadOnly();

    public static Author Create(string firstName, string lastName, int? birthYear)
    {
        return new Author
        {
            Id = Guid.CreateVersion7(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            BirthYear = birthYear
        };
    }

    public void Update(string firstName, string lastName, int? birthYear)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        BirthYear = birthYear;
    }
}
