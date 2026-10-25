using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using LibraryApi.Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Application.Loans.ReturnBook;

public sealed record ReturnBookCommand(Guid LoanId) : ICommand<CommandResult<EmptyResult>>;

public sealed class ReturnBookCommandHandler(IApplicationDbContext dbContext, TimeProvider timeProvider)
    : ICommandHandler<ReturnBookCommand, CommandResult<EmptyResult>>
{
    public async ValueTask<CommandResult<EmptyResult>> Handle(
        ReturnBookCommand command,
        CancellationToken cancellationToken)
    {
        var loan = await dbContext.Loans.SingleOrDefaultAsync(l => l.Id == command.LoanId, cancellationToken);
        if (loan is null)
            return CommandResult.NotFound<EmptyResult>("Loan");

        try
        {
            loan.Return(timeProvider.GetUtcNow().UtcDateTime);
        }
        catch (DomainException ex)
        {
            return CommandResult.Conflict<EmptyResult>(ex.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return CommandResult.Success();
    }
}
