using FluentValidation;

namespace LibraryApi.Application.Loans.BorrowBook;

public sealed class BorrowBookCommandValidator : AbstractValidator<BorrowBookCommand>
{
    public BorrowBookCommandValidator()
    {
        RuleFor(x => x.BookId)
            .NotEmpty().WithMessage("Book is required");

        RuleFor(x => x.MemberId)
            .NotEmpty().WithMessage("Member is required");
    }
}
