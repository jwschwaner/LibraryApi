using LibraryApi.Application.Interfaces;
using LibraryApi.Application.Results;
using LibraryApi.Domain.Authors;
using Mediator;

namespace LibraryApi.Application.Authors.CreateAuthor;

public sealed record CreateAuthorCommand(string FirstName, string LastName, int? BirthYear)
    : ICommand<CommandResult<AuthorResponse>>;

public sealed class CreateAuthorCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<CreateAuthorCommand, CommandResult<AuthorResponse>>
{
    public async ValueTask<CommandResult<AuthorResponse>> Handle(
        CreateAuthorCommand command,
        CancellationToken cancellationToken)
    {
        var author = Author.Create(command.FirstName, command.LastName, command.BirthYear);

        dbContext.Authors.Add(author);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CommandResult.Created(new AuthorResponse(author.Id, author.FirstName, author.LastName, author.BirthYear));
    }
}
