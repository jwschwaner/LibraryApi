using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using LibraryApi.Domain.Common;
using Mediator;
using MongoDB.Driver;

namespace LibraryApi.Application.Books.UpdateBook;

public sealed record UpdateBookCommand(Guid Id, string Title, int PublishedYear, int TotalCopies)
    : ICommand<CommandResult<EmptyResult>>;

public sealed class UpdateBookCommandHandler(IApplicationDbContext dbContext, TimeProvider timeProvider)
    : ICommandHandler<UpdateBookCommand, CommandResult<EmptyResult>>
{
    public async ValueTask<CommandResult<EmptyResult>> Handle(
        UpdateBookCommand command,
        CancellationToken cancellationToken)
    {
        var book = await dbContext.Books.Find(b => b.Id == command.Id).SingleOrDefaultAsync(cancellationToken);
        if (book is null)
            return CommandResult.NotFound<EmptyResult>("Book");

        var activeLoans = (int)await dbContext.Loans.CountDocumentsAsync(
            l => l.BookId == command.Id && l.ReturnedAtUtc == null,
            cancellationToken: cancellationToken);

        try
        {
            book.Update(command.Title, command.PublishedYear, command.TotalCopies, activeLoans,
                timeProvider.GetUtcNow().UtcDateTime);
        }
        catch (DomainException ex)
        {
            return CommandResult.Conflict<EmptyResult>(ex.Message);
        }

        await dbContext.Books.ReplaceOneAsync(b => b.Id == book.Id, book, cancellationToken: cancellationToken);

        return CommandResult.Success();
    }
}
