using LibraryApi.Application.Authors.UpdateAuthor;

namespace LibraryApi.Api.Controllers.Authors.Requests;

public sealed record UpdateAuthorRequest(string FirstName, string LastName, int? BirthYear)
{
    public UpdateAuthorCommand ToCommand(Guid id) => new(id, FirstName, LastName, BirthYear);
}
