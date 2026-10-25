using FluentValidation;

namespace LibraryApi.Application.Books.CreateBook;

public sealed class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Isbn)
            .NotEmpty().WithMessage("ISBN is required")
            .Matches(@"^\d{13}$").WithMessage("ISBN must be 13 digits without dashes");

        RuleFor(x => x.PublishedYear)
            .InclusiveBetween(1450, DateTime.UtcNow.Year)
            .WithMessage("Published year must be between 1450 and the current year");

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("Author is required");

        RuleFor(x => x.TotalCopies)
            .GreaterThanOrEqualTo(1).WithMessage("A book must have at least one copy");
    }
}
