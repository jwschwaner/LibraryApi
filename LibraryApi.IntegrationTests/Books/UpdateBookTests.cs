using System.Net;
using LibraryApi.Api.Controllers.Books.Requests;
using LibraryApi.Application.Books;
using LibraryApi.Domain.Books;
using LibraryApi.Tests.Common.Builders;
using LibraryApi.Tests.Common.Http;
using LibraryApi.Tests.Common.MongoDb;

namespace LibraryApi.IntegrationTests.Books;

[Collection(MongoDbFixture.CollectionName)]
public sealed class UpdateBookTests(MongoDbFixture mongoDb) : IntegrationTestBase(mongoDb)
{
    [Fact]
    public async Task Given_a_book_When_updating_it_Then_the_changes_are_stored()
    {
        // Arrange
        var author = AuthorBuilder.Simple().Build();
        var book = BookBuilder.Simple().By(author).WithTitle("Draft").WithTotalCopies(1).Build();
        await PersistAsync(author, book);

        // Act
        var response = await PutAsync<object>($"/books/{book.Id}", new UpdateBookRequest("Final", 2001, 4));

        // Assert
        CommonAssertions.StatusCodeMatches(response, HttpStatusCode.NoContent);
        var updated = (await GetAsync<BookResponse>($"/books/{book.Id}")).Content!;
        Assert.Equal(("Final", 2001, 4), (updated.Title, updated.PublishedYear, updated.TotalCopies));
    }

    [Fact]
    public async Task Given_copies_on_loan_When_reducing_the_copies_below_them_Then_conflict_is_returned()
    {
        // Arrange
        var author = AuthorBuilder.Simple().Build();
        var book = BookBuilder.Simple().By(author).WithTotalCopies(2).Build();
        var member = MemberBuilder.Simple().Build();
        await PersistAsync(
            author, book, member, LoanBuilder.Of(book, member).Build(), LoanBuilder.Of(book, member).Build());

        // Act
        var response = await PutAsync<object>($"/books/{book.Id}", new UpdateBookRequest(book.Title, 2000, 1));

        // Assert
        CommonAssertions.IsConflict(response, "Cannot reduce to 1 copies while 2 are on loan", BookErrors.CopiesOnLoan);
    }
}
