using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using LibraryApi.Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Application.Books.UpdateBook;

public sealed record UpdateBookCommand(Guid Id, string Title, int PublishedYear, int TotalCopies)
    : ICommand<CommandResult<EmptyResult>>;

public sealed class UpdateBookCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<UpdateBookCommand, CommandResult<EmptyResult>>
{
    public async ValueTask<CommandResult<EmptyResult>> Handle(
        UpdateBookCommand command,
        CancellationToken cancellationToken)
    {
        var book = await dbContext.Books.SingleOrDefaultAsync(b => b.Id == command.Id, cancellationToken);
        if (book is null)
            return CommandResult.NotFound<EmptyResult>("Book");

        var activeLoans = await dbContext.Loans
            .CountAsync(l => l.BookId == command.Id && l.ReturnedAtUtc == null, cancellationToken);

        try
        {
            book.Update(command.Title, command.PublishedYear, command.TotalCopies, activeLoans);
        }
        catch (DomainException ex)
        {
            return CommandResult.Conflict<EmptyResult>(ex.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return CommandResult.Success();
    }
}
