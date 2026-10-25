namespace LibraryApi.Domain.Books;

/// <summary>
/// <see cref="Common.DomainException"/> codes for book rules that the application maps to
/// specific HTTP outcomes.
/// </summary>
public static class BookErrors
{
    public const string InvalidCopyCount = "Book.InvalidCopyCount";
    public const string CopiesOnLoan = "Book.CopiesOnLoan";
}
