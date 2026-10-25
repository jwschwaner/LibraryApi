using LibraryApi.Application.Books.UpdateBook;

namespace LibraryApi.Api.Controllers.Books.Requests;

public sealed record UpdateBookRequest(string Title, int PublishedYear, int TotalCopies)
{
    public UpdateBookCommand ToCommand(Guid id) => new(id, Title, PublishedYear, TotalCopies);
}
