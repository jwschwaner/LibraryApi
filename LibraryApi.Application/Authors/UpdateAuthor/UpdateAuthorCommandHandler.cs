using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Application.Authors.UpdateAuthor;

public sealed record UpdateAuthorCommand(Guid Id, string FirstName, string LastName, int? BirthYear)
    : ICommand<CommandResult<EmptyResult>>;

public sealed class UpdateAuthorCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<UpdateAuthorCommand, CommandResult<EmptyResult>>
{
    public async ValueTask<CommandResult<EmptyResult>> Handle(
        UpdateAuthorCommand command,
        CancellationToken cancellationToken)
    {
        var author = await dbContext.Authors.SingleOrDefaultAsync(a => a.Id == command.Id, cancellationToken);
        if (author is null)
            return CommandResult.NotFound<EmptyResult>("Author");

        author.Update(command.FirstName, command.LastName, command.BirthYear);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CommandResult.Success();
    }
}
