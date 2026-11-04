using LibraryApi.Domain.Common;

namespace LibraryApi.Domain.Books;

public sealed class Book : AuditedEntity
{
    private Book() { }

    public string Title { get; private set; } = null!;
    public string Isbn { get; private set; } = null!;
    public int PublishedYear { get; private set; }
    public Guid AuthorId { get; private set; }

    /// <summary>How many physical copies the library owns. Availability is derived from active loans.</summary>
    public int TotalCopies { get; private set; }

    public static Book Create(string title, string isbn, int publishedYear, Guid authorId, int totalCopies,
        DateTime nowUtc)
    {
        if (totalCopies < 1)
            throw new DomainException(BookErrors.InvalidCopyCount, "A book must have at least one copy");

        var book = new Book
        {
            Id = Guid.CreateVersion7(),
            Title = title.Trim(),
            Isbn = isbn.Trim(),
            PublishedYear = publishedYear,
            AuthorId = authorId,
            TotalCopies = totalCopies
        };
        book.MarkCreated(nowUtc);
        return book;
    }

    /// <param name="activeLoans">Copies currently lent out; the library can't own fewer copies than that.</param>
    public void Update(string title, int publishedYear, int totalCopies, int activeLoans, DateTime nowUtc)
    {
        if (totalCopies < 1)
            throw new DomainException(BookErrors.InvalidCopyCount, "A book must have at least one copy");
        if (totalCopies < activeLoans)
            throw new DomainException(BookErrors.CopiesOnLoan,
                $"Cannot reduce to {totalCopies} copies while {activeLoans} are on loan");

        Title = title.Trim();
        PublishedYear = publishedYear;
        TotalCopies = totalCopies;
        MarkUpdated(nowUtc);
    }
}
