using FluentValidation;

namespace LibraryApi.Application.Books.UpdateBook;

public sealed class UpdateBookCommandValidator : AbstractValidator<UpdateBookCommand>
{
    public UpdateBookCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.PublishedYear)
            .InclusiveBetween(1450, DateTime.UtcNow.Year)
            .WithMessage("Published year must be between 1450 and the current year");

        RuleFor(x => x.TotalCopies)
            .GreaterThanOrEqualTo(1).WithMessage("A book must have at least one copy");
    }
}
