using LibraryApi.Application.Authors;
using LibraryApi.Tests.Common.Builders;
using LibraryApi.Tests.Common.Http;
using LibraryApi.Tests.Common.MongoDb;

namespace LibraryApi.IntegrationTests.Authors;

[Collection(MongoDbFixture.CollectionName)]
public sealed class ListAuthorsTests(MongoDbFixture mongoDb) : IntegrationTestBase(mongoDb)
{
    [Fact]
    public async Task Given_several_authors_When_listing_authors_Then_they_are_sorted_by_last_then_first_name()
    {
        // Arrange
        await PersistAsync(
            AuthorBuilder.Simple().WithFirstName("Karen").WithLastName("Blixen").Build(),
            AuthorBuilder.Simple().WithFirstName("Ursula").WithLastName("Le Guin").Build(),
            AuthorBuilder.Simple().WithFirstName("Anne").WithLastName("Blixen").Build());

        // Act
        var response = await GetAsync<List<AuthorResponse>>("/authors");

        // Assert
        CommonAssertions.IsSuccessStatusCode(response);
        Assert.Equal(
            ["Anne Blixen", "Karen Blixen", "Ursula Le Guin"],
            response.Content!.Select(a => $"{a.FirstName} {a.LastName}"));
    }

    [Fact]
    public async Task Given_an_author_without_a_birth_year_When_listing_authors_Then_the_birth_year_is_null()
    {
        // Arrange
        await PersistAsync(AuthorBuilder.Simple().WithBirthYear(null).Build());

        // Act
        var response = await GetAsync<List<AuthorResponse>>("/authors");

        // Assert
        CommonAssertions.IsSuccessStatusCode(response);
        Assert.Null(Assert.Single(response.Content!).BirthYear);
    }
}
