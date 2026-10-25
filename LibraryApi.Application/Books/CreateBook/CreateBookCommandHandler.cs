using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using LibraryApi.Domain.Books;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Application.Books.CreateBook;

public sealed record CreateBookCommand(
    string Title,
    string Isbn,
    int PublishedYear,
    Guid AuthorId,
    int TotalCopies
) : ICommand<CommandResult<CreateBookResponse>>;

public sealed record CreateBookResponse(Guid Id, string Title, string Isbn, int PublishedYear, Guid AuthorId, int TotalCopies);

public sealed class CreateBookCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<CreateBookCommand, CommandResult<CreateBookResponse>>
{
    public async ValueTask<CommandResult<CreateBookResponse>> Handle(
        CreateBookCommand command,
        CancellationToken cancellationToken)
    {
        var authorExists = await dbContext.Authors.AnyAsync(a => a.Id == command.AuthorId, cancellationToken);
        if (!authorExists)
            return CommandResult.NotFound<CreateBookResponse>("Author");

        var isbnTaken = await dbContext.Books.AnyAsync(b => b.Isbn == command.Isbn, cancellationToken);
        if (isbnTaken)
            return CommandResult.Conflict<CreateBookResponse>($"A book with ISBN {command.Isbn} already exists");

        var book = Book.Create(command.Title, command.Isbn, command.PublishedYear, command.AuthorId, command.TotalCopies);

        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CommandResult.Created(new CreateBookResponse(
            book.Id, book.Title, book.Isbn, book.PublishedYear, book.AuthorId, book.TotalCopies));
    }
}
