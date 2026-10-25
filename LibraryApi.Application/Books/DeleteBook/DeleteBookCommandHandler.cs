using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Application.Books.DeleteBook;

public sealed record DeleteBookCommand(Guid Id) : ICommand<CommandResult<EmptyResult>>;

public sealed class DeleteBookCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<DeleteBookCommand, CommandResult<EmptyResult>>
{
    public async ValueTask<CommandResult<EmptyResult>> Handle(
        DeleteBookCommand command,
        CancellationToken cancellationToken)
    {
        var book = await dbContext.Books.SingleOrDefaultAsync(b => b.Id == command.Id, cancellationToken);
        if (book is null)
            return CommandResult.NotFound<EmptyResult>("Book");

        // Loans keep their book, so a book that has ever been lent out stays in the catalogue
        var hasLoans = await dbContext.Loans.AnyAsync(l => l.BookId == command.Id, cancellationToken);
        if (hasLoans)
            return CommandResult.Conflict<EmptyResult>("Cannot delete a book that has loan history");

        dbContext.Books.Remove(book);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CommandResult.Success();
    }
}
