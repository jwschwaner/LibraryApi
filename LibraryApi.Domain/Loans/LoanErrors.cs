namespace LibraryApi.Domain.Loans;

/// <summary>
/// <see cref="Common.DomainException"/> codes for lending rules that the application maps to
/// specific HTTP outcomes.
/// </summary>
public static class LoanErrors
{
    public const string NoCopiesAvailable = "Loan.NoCopiesAvailable";
    public const string MemberHasOverdueLoans = "Loan.MemberHasOverdueLoans";
    public const string LoanLimitReached = "Loan.LoanLimitReached";
    public const string AlreadyReturned = "Loan.AlreadyReturned";
}
