using System.Net;
using LibraryApi.Application.Books;
using LibraryApi.Domain.Loans;
using LibraryApi.Tests.Common;
using LibraryApi.Tests.Common.Builders;
using LibraryApi.Tests.Common.Http;
using LibraryApi.Tests.Common.MongoDb;

namespace LibraryApi.IntegrationTests.Loans;

[Collection(MongoDbFixture.CollectionName)]
public sealed class ReturnBookTests(MongoDbFixture mongoDb) : IntegrationTestBase(mongoDb)
{
    [Fact]
    public async Task Given_an_active_loan_When_returning_the_book_Then_the_copy_is_available_again()
    {
        // Arrange
        var author = AuthorBuilder.Simple().Build();
        var book = BookBuilder.Simple().By(author).Build();
        var member = MemberBuilder.Simple().Build();
        var loan = LoanBuilder.Of(book, member).Build();
        await PersistAsync(author, book, member, loan);

        // Act
        var response = await PostAsync<object>($"/loans/{loan.Id}/return");

        // Assert
        CommonAssertions.StatusCodeMatches(response, HttpStatusCode.NoContent);
        var after = await GetAsync<BookResponse>($"/books/{book.Id}");
        Assert.Equal(1, after.Content!.AvailableCopies);
    }

    [Fact]
    public async Task Given_a_returned_loan_When_returning_it_again_Then_conflict_is_returned()
    {
        // Arrange
        var author = AuthorBuilder.Simple().Build();
        var book = BookBuilder.Simple().By(author).Build();
        var member = MemberBuilder.Simple().Build();
        var loan = LoanBuilder.Of(book, member).Returned().Build();
        await PersistAsync(author, book, member, loan);

        // Act
        var response = await PostAsync<object>($"/loans/{loan.Id}/return");

        // Assert
        CommonAssertions.IsConflict(response, "This loan has already been returned", LoanErrors.AlreadyReturned);
    }

    [Fact]
    public async Task Given_an_unknown_loan_id_When_returning_it_Then_not_found_is_returned()
    {
        // Act
        var response = await PostAsync<object>($"/loans/{TestConstants.UnknownId}/return");

        // Assert
        CommonAssertions.StatusCodeMatches(response, HttpStatusCode.NotFound);
    }
}
