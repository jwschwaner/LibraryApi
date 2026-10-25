namespace LibraryApi.Application.Books;

public sealed record BookResponse(
    Guid Id,
    string Title,
    string Isbn,
    int PublishedYear,
    Guid AuthorId,
    string AuthorName,
    int TotalCopies,
    int AvailableCopies
);
