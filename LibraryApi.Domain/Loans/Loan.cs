using LibraryApi.Domain.Common;

namespace LibraryApi.Domain.Loans;

public sealed class Loan : AuditedEntity
{
    public const int LoanPeriodDays = 14;
    public const int MaxActiveLoansPerMember = 3;

    private Loan() { }

    public Guid BookId { get; private set; }
    public Guid MemberId { get; private set; }
    public DateTime BorrowedAtUtc { get; private set; }
    public DateOnly DueOn { get; private set; }
    public DateTime? ReturnedAtUtc { get; private set; }

    public bool IsReturned => ReturnedAtUtc is not null;

    /// <summary>
    /// Lends a copy of a book to a member. The caller supplies a snapshot of the current
    /// loan counts, so the lending rules live here rather than in the handler.
    /// </summary>
    /// <param name="totalCopies">Copies of the book the library owns.</param>
    /// <param name="activeLoansForBook">Copies of the book currently lent out.</param>
    /// <param name="activeLoansForMember">Books the member currently has at home.</param>
    /// <param name="memberHasOverdueLoans">Whether any of those are past their due date.</param>
    public static Loan Create(
        Guid bookId,
        Guid memberId,
        int totalCopies,
        int activeLoansForBook,
        int activeLoansForMember,
        bool memberHasOverdueLoans,
        DateTime nowUtc)
    {
        if (activeLoansForBook >= totalCopies)
            throw new DomainException(LoanErrors.NoCopiesAvailable, "All copies of this book are on loan");
        if (memberHasOverdueLoans)
            throw new DomainException(LoanErrors.MemberHasOverdueLoans,
                "Member must return overdue books before borrowing more");
        if (activeLoansForMember >= MaxActiveLoansPerMember)
            throw new DomainException(LoanErrors.LoanLimitReached,
                $"Member already has {MaxActiveLoansPerMember} books on loan");

        var loan = new Loan
        {
            Id = Guid.CreateVersion7(),
            BookId = bookId,
            MemberId = memberId,
            BorrowedAtUtc = nowUtc,
            DueOn = DateOnly.FromDateTime(nowUtc).AddDays(LoanPeriodDays)
        };
        loan.MarkCreated(nowUtc);
        return loan;
    }

    public void Return(DateTime nowUtc)
    {
        if (IsReturned)
            throw new DomainException(LoanErrors.AlreadyReturned, "This loan has already been returned");

        ReturnedAtUtc = nowUtc;
        MarkUpdated(nowUtc);
    }
}
