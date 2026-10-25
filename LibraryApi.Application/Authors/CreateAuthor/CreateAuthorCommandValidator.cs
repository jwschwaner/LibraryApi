using FluentValidation;

namespace LibraryApi.Application.Authors.CreateAuthor;

public sealed class CreateAuthorCommandValidator : AbstractValidator<CreateAuthorCommand>
{
    public CreateAuthorCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.BirthYear)
            .InclusiveBetween(0, DateTime.UtcNow.Year).When(x => x.BirthYear is not null)
            .WithMessage("Birth year must be between 0 and the current year");
    }
}
