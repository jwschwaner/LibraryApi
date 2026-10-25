using LibraryApi.Api.Controllers.Base;
using LibraryApi.Api.Controllers.Books.Requests;
using LibraryApi.Api.Extensions;
using LibraryApi.Application.Books.DeleteBook;
using LibraryApi.Application.Books.GetBook;
using LibraryApi.Application.Books.SearchBooks;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Api.Controllers;

public class BooksController(ISender sender) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> SearchBooks(
        [FromQuery] string? search,
        [FromQuery] bool availableOnly,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SearchBooksQuery(search, availableOnly), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBook(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookQuery(id), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateBook([FromBody] CreateBookRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(), cancellationToken);
        return result.ToCreatedResult(book => $"/books/{book.Id}");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateBook(Guid id, [FromBody] UpdateBookRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.ToCommand(id), cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBook(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteBookCommand(id), cancellationToken);
        return result.ToActionResult();
    }
}
