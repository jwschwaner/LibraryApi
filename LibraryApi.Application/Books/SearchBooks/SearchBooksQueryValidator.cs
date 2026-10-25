using FluentValidation;

namespace LibraryApi.Application.Books.SearchBooks;

public sealed class SearchBooksQueryValidator : AbstractValidator<SearchBooksQuery>
{
    public SearchBooksQueryValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(100).WithMessage("Search must not exceed 100 characters");
    }
}
