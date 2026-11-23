using System.Net;
using LibraryApi.Application.Books;
using LibraryApi.Tests.Common;
using LibraryApi.Tests.Common.Builders;
using LibraryApi.Tests.Common.Http;
using LibraryApi.Tests.Common.MongoDb;

namespace LibraryApi.IntegrationTests.Books;

[Collection(MongoDbFixture.CollectionName)]
public sealed class GetBookTests(MongoDbFixture mongoDb) : IntegrationTestBase(mongoDb)
{
    [Fact]
    public async Task Given_a_book_When_requesting_it_Then_the_book_and_its_author_name_are_returned()
    {
        // Arrange
        var author = AuthorBuilder.Typical().Build();
        var book = BookBuilder.Typical().By(author).WithIsbn("9780547773742").Build();
        await PersistAsync(author, book);

        // Act
        var response = await GetAsync<BookResponse>($"/books/{book.Id}");

        // Assert
        CommonAssertions.IsSuccessStatusCode(response);
        Assert.Equal(
            new BookResponse(book.Id, "A Wizard of Earthsea", "9780547773742", 1968, author.Id, "Ursula K. Le Guin", 2, 2),
            response.Content);
    }

    [Fact]
    public async Task Given_active_and_returned_loans_When_requesting_the_book_Then_only_active_loans_reduce_the_available_copies()
    {
        // Arrange
        var author = AuthorBuilder.Simple().Build();
        var book = BookBuilder.Simple().By(author).WithTotalCopies(3).Build();
        var member = MemberBuilder.Simple().Build();
        await PersistAsync(
            author,
            book,
            member,
            LoanBuilder.Of(book, member).Build(),
            LoanBuilder.Of(book, member).Overdue().Build(),
            LoanBuilder.Of(book, member).Returned().Build());

        // Act
        var response = await GetAsync<BookResponse>($"/books/{book.Id}");

        // Assert
        CommonAssertions.IsSuccessStatusCode(response);
        Assert.Equal(3, response.Content!.TotalCopies);
        Assert.Equal(1, response.Content.AvailableCopies);
    }

    [Fact]
    public async Task Given_an_unknown_book_id_When_requesting_the_book_Then_not_found_is_returned()
    {
        // Act
        var response = await GetAsync<BookResponse>($"/books/{TestConstants.UnknownId}");

        // Assert
        CommonAssertions.StatusCodeMatches(response, HttpStatusCode.NotFound);
    }
}
