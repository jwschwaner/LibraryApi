using System.Net;
using LibraryApi.Api.Controllers.Loans.Requests;
using LibraryApi.Application.Loans;
using LibraryApi.Domain.Authors;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Loans;
using LibraryApi.Domain.Members;
using LibraryApi.Tests.Common;
using LibraryApi.Tests.Common.Builders;
using LibraryApi.Tests.Common.Http;
using LibraryApi.Tests.Common.MongoDb;

namespace LibraryApi.IntegrationTests.Loans;

[Collection(MongoDbFixture.CollectionName)]
public sealed class BorrowBookTests(MongoDbFixture mongoDb) : IntegrationTestBase(mongoDb)
{
    private readonly Author _author = AuthorBuilder.Simple().Build();
    private readonly Member _member = MemberBuilder.Simple().Build();

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await PersistAsync(_author, _member);
    }

    [Fact]
    public async Task Given_an_available_book_When_borrowing_it_Then_the_loan_is_due_in_14_days()
    {
        // Arrange
        var book = await SeedBookAsync();

        // Act
        var response = await PostAsync<LoanResponse>("/loans", new BorrowBookRequest(book.Id, _member.Id));

        // Assert
        CommonAssertions.StatusCodeMatches(response, HttpStatusCode.Created);
        var loan = response.Content!;
        Assert.Equal(TestConstants.NowUtc, loan.BorrowedAtUtc);
        Assert.Equal(TestConstants.Today.AddDays(14), loan.DueOn);
        Assert.Null(loan.ReturnedAtUtc);
    }

    [Fact]
    public async Task Given_every_copy_on_loan_When_borrowing_the_book_Then_conflict_is_returned()
    {
        // Arrange
        var book = await SeedBookAsync();
        var otherMember = MemberBuilder.Simple().Build();
        await PersistAsync(otherMember, LoanBuilder.Of(book, otherMember).Build());

        // Act
        var response = await PostAsync<LoanResponse>("/loans", new BorrowBookRequest(book.Id, _member.Id));

        // Assert
        CommonAssertions.IsConflict(response, "All copies of this book are on loan", LoanErrors.NoCopiesAvailable);
    }

    [Fact]
    public async Task Given_a_member_with_an_overdue_loan_When_borrowing_a_book_Then_conflict_is_returned()
    {
        // Arrange
        var overdueBook = await SeedBookAsync();
        var book = await SeedBookAsync();
        await PersistAsync(LoanBuilder.Of(overdueBook, _member).Overdue().Build());

        // Act
        var response = await PostAsync<LoanResponse>("/loans", new BorrowBookRequest(book.Id, _member.Id));

        // Assert
        CommonAssertions.IsConflict(response, "Member must return overdue books before borrowing more", LoanErrors.MemberHasOverdueLoans);
    }

    [Fact]
    public async Task Given_a_member_at_the_loan_limit_When_borrowing_a_book_Then_conflict_is_returned()
    {
        // Arrange
        for (var i = 0; i < 3; i++)
            await PersistAsync(LoanBuilder.Of(await SeedBookAsync(), _member).Build());
        var book = await SeedBookAsync();

        // Act
        var response = await PostAsync<LoanResponse>("/loans", new BorrowBookRequest(book.Id, _member.Id));

        // Assert
        CommonAssertions.IsConflict(response, "Member already has 3 books on loan", LoanErrors.LoanLimitReached);
    }

    [Fact]
    public async Task Given_an_unknown_member_When_borrowing_a_book_Then_not_found_is_returned()
    {
        // Arrange
        var book = await SeedBookAsync();

        // Act
        var response = await PostAsync<LoanResponse>("/loans", new BorrowBookRequest(book.Id, TestConstants.UnknownId));

        // Assert
        CommonAssertions.StatusCodeMatches(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Given_a_body_that_is_not_json_When_borrowing_a_book_Then_bad_request_is_returned_as_problem_details()
    {
        // Act
        var response = await PostRawAsync<LoanResponse>("/loans", "{ not json");

        // Assert: MVC rejects the body itself, in the same ProblemDetails format as our own errors
        CommonAssertions.StatusCodeMatches(response, HttpStatusCode.BadRequest);
        Assert.Equal(400, response.Error?.Status);
        Assert.NotNull(response.Error?.Title);
    }

    private async Task<Book> SeedBookAsync()
    {
        var book = BookBuilder.Simple().By(_author).Build();
        await PersistAsync(book);
        return book;
    }
}
