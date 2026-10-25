using LibraryApi.Application.Loans.BorrowBook;

namespace LibraryApi.Api.Controllers.Loans.Requests;

public sealed record BorrowBookRequest(Guid BookId, Guid MemberId)
{
    public BorrowBookCommand ToCommand() => new(BookId, MemberId);
}
