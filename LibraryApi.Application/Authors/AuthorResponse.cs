namespace LibraryApi.Application.Authors;

public sealed record AuthorResponse(Guid Id, string FirstName, string LastName, int? BirthYear);
