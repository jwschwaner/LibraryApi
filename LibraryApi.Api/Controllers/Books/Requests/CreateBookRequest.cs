using LibraryApi.Application.Books.CreateBook;

namespace LibraryApi.Api.Controllers.Books.Requests;

public sealed record CreateBookRequest(string Title, string Isbn, int PublishedYear, Guid AuthorId, int TotalCopies)
{
    public CreateBookCommand ToCommand() => new(Title, Isbn, PublishedYear, AuthorId, TotalCopies);
}
