using LibraryApi.Domain.Authors;
using LibraryApi.Domain.Books;

namespace LibraryApi.Tests.Common.Builders;

/// <summary>
/// Builds books through <see cref="Book.Create"/>. A book needs an author, so pass the author
/// the test seeds with <see cref="By"/>; ISBNs are unique per builder because the collection has a
/// unique index on them.
/// </summary>
public sealed class BookBuilder
{
    private static int _nextIsbn = 1;

    private string _title = "Test Book";
    private string _isbn = NextIsbn();
    private int _publishedYear = 2000;
    private Guid _authorId;
    private int _totalCopies = 1;
    private DateTime _createdAtUtc = TestConstants.NowUtc;

    private BookBuilder() { }

    public static BookBuilder Simple() => new();

    public static BookBuilder Typical() => Simple()
        .WithTitle("A Wizard of Earthsea")
        .WithPublishedYear(1968)
        .WithTotalCopies(2);

    public BookBuilder By(Author author)
    {
        _authorId = author.Id;
        return this;
    }

    public BookBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public BookBuilder WithIsbn(string isbn)
    {
        _isbn = isbn;
        return this;
    }

    public BookBuilder WithPublishedYear(int publishedYear)
    {
        _publishedYear = publishedYear;
        return this;
    }

    public BookBuilder WithTotalCopies(int totalCopies)
    {
        _totalCopies = totalCopies;
        return this;
    }

    public BookBuilder WithCreatedAtUtc(DateTime createdAtUtc)
    {
        _createdAtUtc = createdAtUtc;
        return this;
    }

    public Book Build()
    {
        if (_authorId == Guid.Empty)
            throw new InvalidOperationException("Call By(author) before Build(): a book needs an author");

        return Book.Create(_title, _isbn, _publishedYear, _authorId, _totalCopies, _createdAtUtc);
    }

    private static string NextIsbn() => $"978{Interlocked.Increment(ref _nextIsbn):D10}";
}
