using LibraryApi.Domain.Common;
using LibraryApi.Domain.Loans;
using LibraryApi.Tests.Common;

namespace LibraryApi.UnitTests.Domain;

public sealed class LoanTests
{
    private static readonly Guid BookId = Guid.NewGuid();
    private static readonly Guid MemberId = Guid.NewGuid();

    public sealed class Create
    {
        [Fact]
        public void Given_a_free_copy_and_a_member_in_good_standing_When_creating_a_loan_Then_it_is_due_in_the_loan_period()
        {
            // Act
            var loan = Loan.Create(BookId, MemberId, totalCopies: 2, activeLoansForBook: 1,
                activeLoansForMember: 0, memberHasOverdueLoans: false, TestConstants.NowUtc);

            // Assert
            Assert.Equal(BookId, loan.BookId);
            Assert.Equal(MemberId, loan.MemberId);
            Assert.Equal(TestConstants.NowUtc, loan.BorrowedAtUtc);
            Assert.Equal(TestConstants.Today.AddDays(Loan.LoanPeriodDays), loan.DueOn);
            Assert.Equal(TestConstants.NowUtc, loan.CreatedAtUtc);
            Assert.False(loan.IsReturned);
        }

        [Fact]
        public void Given_every_copy_on_loan_When_creating_a_loan_Then_it_is_refused()
        {
            // Act
            var ex = Assert.Throws<DomainException>(() => Loan.Create(BookId, MemberId, totalCopies: 2,
                activeLoansForBook: 2, activeLoansForMember: 0, memberHasOverdueLoans: false, TestConstants.NowUtc));

            // Assert
            Assert.Equal(LoanErrors.NoCopiesAvailable, ex.Code);
        }

        [Fact]
        public void Given_a_member_with_overdue_loans_When_creating_a_loan_Then_it_is_refused()
        {
            // Act
            var ex = Assert.Throws<DomainException>(() => Loan.Create(BookId, MemberId, totalCopies: 1,
                activeLoansForBook: 0, activeLoansForMember: 1, memberHasOverdueLoans: true, TestConstants.NowUtc));

            // Assert
            Assert.Equal(LoanErrors.MemberHasOverdueLoans, ex.Code);
        }

        [Theory]
        [InlineData(Loan.MaxActiveLoansPerMember)]
        [InlineData(Loan.MaxActiveLoansPerMember + 1)]
        public void Given_a_member_at_the_loan_limit_When_creating_a_loan_Then_it_is_refused(int activeLoansForMember)
        {
            // Act
            var ex = Assert.Throws<DomainException>(() => Loan.Create(BookId, MemberId, totalCopies: 1,
                activeLoansForBook: 0, activeLoansForMember, memberHasOverdueLoans: false, TestConstants.NowUtc));

            // Assert
            Assert.Equal(LoanErrors.LoanLimitReached, ex.Code);
        }
    }

    public sealed class Return
    {
        [Fact]
        public void Given_an_active_loan_When_returning_it_Then_it_is_stamped_as_returned()
        {
            // Arrange
            var loan = Loan.Create(BookId, MemberId, 1, 0, 0, false, TestConstants.NowUtc);
            var returnedAt = TestConstants.NowUtc.AddDays(3);

            // Act
            loan.Return(returnedAt);

            // Assert
            Assert.True(loan.IsReturned);
            Assert.Equal(returnedAt, loan.ReturnedAtUtc);
            Assert.Equal(returnedAt, loan.UpdatedAtUtc);
        }

        [Fact]
        public void Given_a_returned_loan_When_returning_it_again_Then_it_is_refused()
        {
            // Arrange
            var loan = Loan.Create(BookId, MemberId, 1, 0, 0, false, TestConstants.NowUtc);
            loan.Return(TestConstants.NowUtc);

            // Act
            var ex = Assert.Throws<DomainException>(() => loan.Return(TestConstants.NowUtc));

            // Assert
            Assert.Equal(LoanErrors.AlreadyReturned, ex.Code);
        }
    }
}
