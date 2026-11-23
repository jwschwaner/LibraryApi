using System.Net;
using LibraryApi.Application.Authors.GetAuthor;
using LibraryApi.Tests.Common;
using LibraryApi.Tests.Common.Builders;
using LibraryApi.Tests.Common.Http;
using LibraryApi.Tests.Common.MongoDb;

namespace LibraryApi.IntegrationTests.Authors;

[Collection(MongoDbFixture.CollectionName)]
public sealed class GetAuthorTests(MongoDbFixture mongoDb) : IntegrationTestBase(mongoDb)
{
    [Fact]
    public async Task Given_an_author_with_books_When_requesting_the_author_Then_the_books_are_sorted_by_published_year()
    {
        // Arrange
        var author = AuthorBuilder.Typical().Build();
        var other = AuthorBuilder.Simple().Build();
        await PersistAsync(
            author,
            other,
            BookBuilder.Simple().By(author).WithTitle("The Dispossessed").WithPublishedYear(1974).Build(),
            BookBuilder.Simple().By(author).WithTitle("A Wizard of Earthsea").WithPublishedYear(1968).Build(),
            BookBuilder.Simple().By(other).WithTitle("Someone else's book").Build());

        // Act
        var response = await GetAsync<AuthorDetailsResponse>($"/authors/{author.Id}");

        // Assert
        CommonAssertions.IsSuccessStatusCode(response);
        var content = response.Content!;
        Assert.Equal(author.Id, content.Id);
        Assert.Equal("Ursula K.", content.FirstName);
        Assert.Equal("Le Guin", content.LastName);
        Assert.Equal(1929, content.BirthYear);
        Assert.Equal(["A Wizard of Earthsea", "The Dispossessed"], content.Books.Select(b => b.Title));
    }

    [Fact]
    public async Task Given_an_unknown_author_id_When_requesting_the_author_Then_not_found_is_returned()
    {
        // Act
        var response = await GetAsync<AuthorDetailsResponse>($"/authors/{TestConstants.UnknownId}");

        // Assert
        CommonAssertions.StatusCodeMatches(response, HttpStatusCode.NotFound);
    }
}
