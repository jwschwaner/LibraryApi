using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using LibraryApi.Domain.Books;
using Mediator;
using MongoDB.Driver;

namespace LibraryApi.Application.Books.CreateBook;

public sealed record CreateBookCommand(
    string Title,
    string Isbn,
    int PublishedYear,
    Guid AuthorId,
    int TotalCopies
) : ICommand<CommandResult<CreateBookResponse>>;

public sealed record CreateBookResponse(Guid Id, string Title, string Isbn, int PublishedYear, Guid AuthorId, int TotalCopies);

public sealed class CreateBookCommandHandler(IApplicationDbContext dbContext, TimeProvider timeProvider)
    : ICommandHandler<CreateBookCommand, CommandResult<CreateBookResponse>>
{
    public async ValueTask<CommandResult<CreateBookResponse>> Handle(
        CreateBookCommand command,
        CancellationToken cancellationToken)
    {
        var authorExists = await dbContext.Authors.Find(a => a.Id == command.AuthorId).AnyAsync(cancellationToken);
        if (!authorExists)
            return CommandResult.NotFound<CreateBookResponse>("Author");

        var isbnTaken = await dbContext.Books.Find(b => b.Isbn == command.Isbn).AnyAsync(cancellationToken);
        if (isbnTaken)
            return CommandResult.Conflict<CreateBookResponse>($"A book with ISBN {command.Isbn} already exists");

        var book = Book.Create(command.Title, command.Isbn, command.PublishedYear, command.AuthorId, command.TotalCopies,
            timeProvider.GetUtcNow().UtcDateTime);

        await dbContext.Books.InsertOneAsync(book, cancellationToken: cancellationToken);

        return CommandResult.Created(new CreateBookResponse(
            book.Id, book.Title, book.Isbn, book.PublishedYear, book.AuthorId, book.TotalCopies));
    }
}
