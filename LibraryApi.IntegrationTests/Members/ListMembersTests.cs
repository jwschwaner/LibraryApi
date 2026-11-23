using LibraryApi.Application.Members;
using LibraryApi.Tests.Common.Builders;
using LibraryApi.Tests.Common.Http;
using LibraryApi.Tests.Common.MongoDb;

namespace LibraryApi.IntegrationTests.Members;

[Collection(MongoDbFixture.CollectionName)]
public sealed class ListMembersTests(MongoDbFixture mongoDb) : IntegrationTestBase(mongoDb)
{
    [Fact]
    public async Task Given_several_members_When_listing_members_Then_they_are_sorted_by_full_name()
    {
        // Arrange
        await PersistAsync(
            MemberBuilder.Simple().WithFullName("Grace Hopper").Build(),
            MemberBuilder.Simple().WithFullName("Ada Lovelace").Build(),
            MemberBuilder.Simple().WithFullName("Alan Turing").Build());

        // Act
        var response = await GetAsync<List<MemberResponse>>("/members");

        // Assert
        CommonAssertions.IsSuccessStatusCode(response);
        Assert.Equal(["Ada Lovelace", "Alan Turing", "Grace Hopper"], response.Content!.Select(m => m.FullName));
    }
}
