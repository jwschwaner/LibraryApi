using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Loans;
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
        var book = await dbContext.Books.Find(Builders<Book>.Filter.Eq(b => b.Id, command.Id)).SingleOrDefaultAsync(cancellationToken);
        if (book is null)
            return CommandResult.NotFound<EmptyResult>("Book");

        var activeLoans = (int)await dbContext.Loans.CountDocumentsAsync(
            Builders<Loan>.Filter.Eq(l => l.BookId, command.Id) & Builders<Loan>.Filter.Eq(l => l.ReturnedAtUtc, null),
            cancellationToken: cancellationToken);

        book.Update(command.Title, command.PublishedYear, command.TotalCopies, activeLoans,
            timeProvider.GetUtcNow().UtcDateTime);

        await dbContext.Books.ReplaceOneAsync(Builders<Book>.Filter.Eq(b => b.Id, book.Id), book, cancellationToken: cancellationToken);

        return CommandResult.Success();
    }
}
