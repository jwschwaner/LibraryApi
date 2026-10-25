using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Application.Authors.DeleteAuthor;

public sealed record DeleteAuthorCommand(Guid Id) : ICommand<CommandResult<EmptyResult>>;

public sealed class DeleteAuthorCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<DeleteAuthorCommand, CommandResult<EmptyResult>>
{
    public async ValueTask<CommandResult<EmptyResult>> Handle(
        DeleteAuthorCommand command,
        CancellationToken cancellationToken)
    {
        var author = await dbContext.Authors.SingleOrDefaultAsync(a => a.Id == command.Id, cancellationToken);
        if (author is null)
            return CommandResult.NotFound<EmptyResult>("Author");

        var hasBooks = await dbContext.Books.AnyAsync(b => b.AuthorId == command.Id, cancellationToken);
        if (hasBooks)
            return CommandResult.Conflict<EmptyResult>("Cannot delete an author who has books in the catalogue");

        dbContext.Authors.Remove(author);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CommandResult.Success();
    }
}
