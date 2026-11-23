using LibraryApi.Application.Loans.OverdueLoans;
using LibraryApi.Tests.Common;
using LibraryApi.Tests.Common.Builders;
using LibraryApi.Tests.Common.Http;
using LibraryApi.Tests.Common.MongoDb;

namespace LibraryApi.IntegrationTests.Loans;

[Collection(MongoDbFixture.CollectionName)]
public sealed class OverdueLoansTests(MongoDbFixture mongoDb) : IntegrationTestBase(mongoDb)
{
    [Fact]
    public async Task Given_loans_in_every_state_When_listing_overdue_loans_Then_only_unreturned_past_due_loans_are_listed_by_due_date()
    {
        // Arrange
        var author = AuthorBuilder.Simple().Build();
        var book = BookBuilder.Simple().By(author).WithTitle("The Hobbit").WithTotalCopies(5).Build();
        var ada = MemberBuilder.Simple().WithFullName("Ada Lovelace").WithEmail("ada@example.com").Build();
        var alan = MemberBuilder.Simple().WithFullName("Alan Turing").WithEmail("alan@example.com").Build();
        var oneDayLate = LoanBuilder.Of(book, ada).Overdue(daysOverdue: 1).Build();
        var tenDaysLate = LoanBuilder.Of(book, alan).Overdue(daysOverdue: 10).Build();
        await PersistAsync(
            author,
            book,
            ada,
            alan,
            oneDayLate,
            tenDaysLate,
            LoanBuilder.Of(book, ada).Build(),
            LoanBuilder.Of(book, alan).Overdue().Returned().Build(),
            // Due today is not overdue yet
            LoanBuilder.Of(book, ada).Overdue(daysOverdue: 0).Build());

        // Act
        var response = await GetAsync<List<OverdueLoanResponse>>("/loans/overdue");

        // Assert
        CommonAssertions.IsSuccessStatusCode(response);
        Assert.Equal(
            [
                new OverdueLoanResponse(
                    tenDaysLate.Id, book.Id, "The Hobbit", alan.Id, "Alan Turing", "alan@example.com",
                    TestConstants.Today.AddDays(-10)),
                new OverdueLoanResponse(
                    oneDayLate.Id, book.Id, "The Hobbit", ada.Id, "Ada Lovelace", "ada@example.com",
                    TestConstants.Today.AddDays(-1))
            ],
            response.Content);
    }

    [Fact]
    public async Task Given_no_overdue_loans_When_listing_overdue_loans_Then_the_list_is_empty()
    {
        // Act
        var response = await GetAsync<List<OverdueLoanResponse>>("/loans/overdue");

        // Assert
        CommonAssertions.IsSuccessStatusCode(response);
        Assert.Empty(response.Content!);
    }
}
