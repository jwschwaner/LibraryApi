using System.Net;
using LibraryApi.Api.Controllers.Books.Requests;
using LibraryApi.Application.Books;
using LibraryApi.Application.Books.CreateBook;
using LibraryApi.Tests.Common;
using LibraryApi.Tests.Common.Builders;
using LibraryApi.Tests.Common.Http;
using LibraryApi.Tests.Common.MongoDb;

namespace LibraryApi.IntegrationTests.Books;

[Collection(MongoDbFixture.CollectionName)]
public sealed class CreateBookTests(MongoDbFixture mongoDb) : IntegrationTestBase(mongoDb)
{
    [Fact]
    public async Task Given_a_valid_book_When_creating_it_Then_it_is_created_with_every_copy_available()
    {
        // Arrange
        var author = AuthorBuilder.Simple().Build();
        await PersistAsync(author);

        // Act
        var response = await PostAsync<CreateBookResponse>(
            "/books", new CreateBookRequest("The Hobbit", "9780547928227", 1937, author.Id, 3));

        // Assert
        CommonAssertions.StatusCodeMatches(response, HttpStatusCode.Created);
        var created = await GetAsync<BookResponse>($"/books/{response.Content!.Id}");
        Assert.Equal(3, created.Content!.AvailableCopies);
    }

    [Fact]
    public async Task Given_an_isbn_that_is_taken_When_creating_a_book_Then_conflict_is_returned()
    {
        // Arrange
        var author = AuthorBuilder.Simple().Build();
        await PersistAsync(author, BookBuilder.Simple().By(author).WithIsbn("9780547928227").Build());

        // Act
        var response = await PostAsync<CreateBookResponse>(
            "/books", new CreateBookRequest("Another Book", "9780547928227", 2000, author.Id, 1));

        // Assert
        CommonAssertions.IsConflict(response, "A book with ISBN 9780547928227 already exists");
    }

    [Fact]
    public async Task Given_an_unknown_author_When_creating_a_book_Then_not_found_is_returned()
    {
        // Act
        var response = await PostAsync<CreateBookResponse>(
            "/books", new CreateBookRequest("Orphan", "9780000000001", 2000, TestConstants.UnknownId, 1));

        // Assert
        CommonAssertions.StatusCodeMatches(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Given_an_isbn_with_dashes_When_creating_a_book_Then_validation_fails()
    {
        // Arrange
        var author = AuthorBuilder.Simple().Build();
        await PersistAsync(author);

        // Act
        var response = await PostAsync<CreateBookResponse>(
            "/books", new CreateBookRequest("The Hobbit", "978-0547928227", 1937, author.Id, 1));

        // Assert
        CommonAssertions.StatusCodeMatches(response, HttpStatusCode.UnprocessableEntity);
    }
}
