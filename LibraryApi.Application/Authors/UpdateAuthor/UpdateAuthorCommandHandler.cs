using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using MongoDB.Driver;

namespace LibraryApi.Application.Authors.UpdateAuthor;

public sealed record UpdateAuthorCommand(Guid Id, string FirstName, string LastName, int? BirthYear)
    : ICommand<CommandResult<EmptyResult>>;

public sealed class UpdateAuthorCommandHandler(IApplicationDbContext dbContext, TimeProvider timeProvider)
    : ICommandHandler<UpdateAuthorCommand, CommandResult<EmptyResult>>
{
    public async ValueTask<CommandResult<EmptyResult>> Handle(
        UpdateAuthorCommand command,
        CancellationToken cancellationToken)
    {
        var author = await dbContext.Authors.Find(a => a.Id == command.Id).SingleOrDefaultAsync(cancellationToken);
        if (author is null)
            return CommandResult.NotFound<EmptyResult>("Author");

        author.Update(command.FirstName, command.LastName, command.BirthYear, timeProvider.GetUtcNow().UtcDateTime);
        await dbContext.Authors.ReplaceOneAsync(a => a.Id == author.Id, author, cancellationToken: cancellationToken);

        return CommandResult.Success();
    }
}
