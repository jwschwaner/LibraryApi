using LibraryApi.Domain.Books;
using LibraryApi.Domain.Loans;
using LibraryApi.Domain.Members;

namespace LibraryApi.Tests.Common.Builders;

/// <summary>
/// Builds loans through <see cref="Loan.Create"/>. The due date follows from the borrow date, so
/// an overdue loan is one borrowed more than <see cref="Loan.LoanPeriodDays"/> days before
/// <see cref="TestConstants.NowUtc"/>: use <see cref="Overdue"/>.
/// </summary>
public sealed class LoanBuilder
{
    private readonly Book _book;
    private readonly Member _member;
    private DateTime _borrowedAtUtc = TestConstants.NowUtc;
    private DateTime? _returnedAtUtc;

    private LoanBuilder(Book book, Member member)
    {
        _book = book;
        _member = member;
    }

    /// <summary>An active loan borrowed now, due in <see cref="Loan.LoanPeriodDays"/> days.</summary>
    public static LoanBuilder Of(Book book, Member member) => new(book, member);

    /// <summary>Due <paramref name="daysOverdue"/> days before today and not returned.</summary>
    public LoanBuilder Overdue(int daysOverdue = 1) =>
        BorrowedAt(TestConstants.NowUtc.AddDays(-(Loan.LoanPeriodDays + daysOverdue)));

    public LoanBuilder BorrowedAt(DateTime borrowedAtUtc)
    {
        _borrowedAtUtc = borrowedAtUtc;
        return this;
    }

    public LoanBuilder Returned(DateTime? returnedAtUtc = null)
    {
        _returnedAtUtc = returnedAtUtc ?? TestConstants.NowUtc;
        return this;
    }

    /// <summary>
    /// Skips the lending rules' counts (every loan is created as if it were the first), so a test
    /// can seed states the API would refuse to create, such as more loans than copies.
    /// </summary>
    public Loan Build()
    {
        var loan = Loan.Create(_book.Id, _member.Id, _book.TotalCopies, 0, 0, false, _borrowedAtUtc);
        if (_returnedAtUtc is { } returnedAtUtc)
            loan.Return(returnedAtUtc);
        return loan;
    }
}
