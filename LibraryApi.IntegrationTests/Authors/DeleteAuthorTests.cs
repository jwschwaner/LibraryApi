using System.Net;
using LibraryApi.Application.Authors.GetAuthor;
using LibraryApi.Tests.Common.Builders;
using LibraryApi.Tests.Common.Http;
using LibraryApi.Tests.Common.MongoDb;

namespace LibraryApi.IntegrationTests.Authors;

[Collection(MongoDbFixture.CollectionName)]
public sealed class DeleteAuthorTests(MongoDbFixture mongoDb) : IntegrationTestBase(mongoDb)
{
    [Fact]
    public async Task Given_an_author_without_books_When_deleting_the_author_Then_the_author_is_gone()
    {
        // Arrange
        var author = AuthorBuilder.Simple().Build();
        await PersistAsync(author);

        // Act
        var response = await DeleteAsync<object>($"/authors/{author.Id}");

        // Assert
        CommonAssertions.StatusCodeMatches(response, HttpStatusCode.NoContent);
        var lookup = await GetAsync<AuthorDetailsResponse>($"/authors/{author.Id}");
        CommonAssertions.StatusCodeMatches(lookup, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Given_an_author_with_books_When_deleting_the_author_Then_conflict_is_returned()
    {
        // Arrange
        var author = AuthorBuilder.Simple().Build();
        await PersistAsync(author, BookBuilder.Simple().By(author).Build());

        // Act
        var response = await DeleteAsync<object>($"/authors/{author.Id}");

        // Assert
        CommonAssertions.IsConflict(response, "Cannot delete an author who has books in the catalogue");
    }
}
