namespace LibraryApi.Application.Loans;

public sealed record LoanResponse(
    Guid Id,
    Guid BookId,
    Guid MemberId,
    DateTime BorrowedAtUtc,
    DateOnly DueOn,
    DateTime? ReturnedAtUtc
);
