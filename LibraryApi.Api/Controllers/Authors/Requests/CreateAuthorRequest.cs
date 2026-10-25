using LibraryApi.Application.Authors.CreateAuthor;

namespace LibraryApi.Api.Controllers.Authors.Requests;

public sealed record CreateAuthorRequest(string FirstName, string LastName, int? BirthYear)
{
    public CreateAuthorCommand ToCommand() => new(FirstName, LastName, BirthYear);
}
