using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Common;
using LibraryApi.Domain.Loans;
using LibraryApi.Domain.Members;
using Mediator;
using MongoDB.Driver;

namespace LibraryApi.Application.Loans.BorrowBook;

public sealed record BorrowBookCommand(Guid BookId, Guid MemberId) : ICommand<CommandResult<LoanResponse>>;

public sealed class BorrowBookCommandHandler(IApplicationDbContext dbContext, TimeProvider timeProvider)
    : ICommandHandler<BorrowBookCommand, CommandResult<LoanResponse>>
{
    public async ValueTask<CommandResult<LoanResponse>> Handle(
        BorrowBookCommand command,
        CancellationToken cancellationToken)
    {
        var book = await dbContext.Books.Find(Builders<Book>.Filter.Eq(b => b.Id, command.BookId)).SingleOrDefaultAsync(cancellationToken);
        if (book is null)
            return CommandResult.NotFound<LoanResponse>("Book");

        var memberExists = await dbContext.Members.Find(Builders<Member>.Filter.Eq(m => m.Id, command.MemberId)).AnyAsync(cancellationToken);
        if (!memberExists)
            return CommandResult.NotFound<LoanResponse>("Member");

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var today = DateOnly.FromDateTime(now);

        // Snapshot what the lending rules need; Loan.Create decides
        var activeLoan = Builders<Loan>.Filter.Eq(l => l.ReturnedAtUtc, null);
        var activeLoansForBook = (int)await dbContext.Loans.CountDocumentsAsync(
            Builders<Loan>.Filter.Eq(l => l.BookId, command.BookId) & activeLoan,
            cancellationToken: cancellationToken);
        var memberLoans = await dbContext.Loans
            .Find(Builders<Loan>.Filter.Eq(l => l.MemberId, command.MemberId) & activeLoan)
            .Project(l => new { l.DueOn })
            .ToListAsync(cancellationToken);

        Loan loan;
        try
        {
            loan = Loan.Create(
                book.Id,
                command.MemberId,
                book.TotalCopies,
                activeLoansForBook,
                memberLoans.Count,
                memberLoans.Any(l => l.DueOn < today),
                now);
        }
        catch (DomainException ex)
        {
            return CommandResult.Conflict<LoanResponse>(ex.Message);
        }

        await dbContext.Loans.InsertOneAsync(loan, cancellationToken: cancellationToken);

        return CommandResult.Created(new LoanResponse(
            loan.Id, loan.BookId, loan.MemberId, loan.BorrowedAtUtc, loan.DueOn, loan.ReturnedAtUtc));
    }
}
