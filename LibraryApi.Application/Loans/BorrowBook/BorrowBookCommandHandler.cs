using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using LibraryApi.Domain.Common;
using LibraryApi.Domain.Loans;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Application.Loans.BorrowBook;

public sealed record BorrowBookCommand(Guid BookId, Guid MemberId) : ICommand<CommandResult<LoanResponse>>;

public sealed class BorrowBookCommandHandler(IApplicationDbContext dbContext, TimeProvider timeProvider)
    : ICommandHandler<BorrowBookCommand, CommandResult<LoanResponse>>
{
    public async ValueTask<CommandResult<LoanResponse>> Handle(
        BorrowBookCommand command,
        CancellationToken cancellationToken)
    {
        var book = await dbContext.Books.SingleOrDefaultAsync(b => b.Id == command.BookId, cancellationToken);
        if (book is null)
            return CommandResult.NotFound<LoanResponse>("Book");

        var memberExists = await dbContext.Members.AnyAsync(m => m.Id == command.MemberId, cancellationToken);
        if (!memberExists)
            return CommandResult.NotFound<LoanResponse>("Member");

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var today = DateOnly.FromDateTime(now);

        // Snapshot what the lending rules need; Loan.Create decides
        var activeLoansForBook = await dbContext.Loans
            .CountAsync(l => l.BookId == command.BookId && l.ReturnedAtUtc == null, cancellationToken);
        var memberLoanDueDates = await dbContext.Loans
            .Where(l => l.MemberId == command.MemberId && l.ReturnedAtUtc == null)
            .Select(l => l.DueOn)
            .ToListAsync(cancellationToken);

        Loan loan;
        try
        {
            loan = Loan.Create(
                book.Id,
                command.MemberId,
                book.TotalCopies,
                activeLoansForBook,
                memberLoanDueDates.Count,
                memberLoanDueDates.Any(dueOn => dueOn < today),
                now);
        }
        catch (DomainException ex)
        {
            return CommandResult.Conflict<LoanResponse>(ex.Message);
        }

        dbContext.Loans.Add(loan);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CommandResult.Created(new LoanResponse(
            loan.Id, loan.BookId, loan.MemberId, loan.BorrowedAtUtc, loan.DueOn, loan.ReturnedAtUtc));
    }
}
