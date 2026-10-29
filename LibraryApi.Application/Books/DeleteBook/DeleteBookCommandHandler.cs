using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using MongoDB.Driver;

namespace LibraryApi.Application.Books.DeleteBook;

public sealed record DeleteBookCommand(Guid Id) : ICommand<CommandResult<EmptyResult>>;

public sealed class DeleteBookCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<DeleteBookCommand, CommandResult<EmptyResult>>
{
    public async ValueTask<CommandResult<EmptyResult>> Handle(
        DeleteBookCommand command,
        CancellationToken cancellationToken)
    {
        var bookExists = await dbContext.Books.Find(b => b.Id == command.Id).AnyAsync(cancellationToken);
        if (!bookExists)
            return CommandResult.NotFound<EmptyResult>("Book");

        // Loans keep their book, so a book that has ever been lent out stays in the catalogue
        var hasLoans = await dbContext.Loans.Find(l => l.BookId == command.Id).AnyAsync(cancellationToken);
        if (hasLoans)
            return CommandResult.Conflict<EmptyResult>("Cannot delete a book that has loan history");

        await dbContext.Books.DeleteOneAsync(b => b.Id == command.Id, cancellationToken);

        return CommandResult.Success();
    }
}
