using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using MongoDB.Driver;

namespace LibraryApi.Application.Authors.DeleteAuthor;

public sealed record DeleteAuthorCommand(Guid Id) : ICommand<CommandResult<EmptyResult>>;

public sealed class DeleteAuthorCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<DeleteAuthorCommand, CommandResult<EmptyResult>>
{
    public async ValueTask<CommandResult<EmptyResult>> Handle(
        DeleteAuthorCommand command,
        CancellationToken cancellationToken)
    {
        var authorExists = await dbContext.Authors.Find(a => a.Id == command.Id).AnyAsync(cancellationToken);
        if (!authorExists)
            return CommandResult.NotFound<EmptyResult>("Author");

        // MongoDB has no foreign keys, so the handler is the only thing protecting the books
        var hasBooks = await dbContext.Books.Find(b => b.AuthorId == command.Id).AnyAsync(cancellationToken);
        if (hasBooks)
            return CommandResult.Conflict<EmptyResult>("Cannot delete an author who has books in the catalogue");

        await dbContext.Authors.DeleteOneAsync(a => a.Id == command.Id, cancellationToken);

        return CommandResult.Success();
    }
}
