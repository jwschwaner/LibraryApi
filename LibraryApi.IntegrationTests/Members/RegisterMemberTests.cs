using System.Net;
using LibraryApi.Api.Controllers.Members.Requests;
using LibraryApi.Application.Members;
using LibraryApi.Tests.Common.Builders;
using LibraryApi.Tests.Common.Http;
using LibraryApi.Tests.Common.MongoDb;

namespace LibraryApi.IntegrationTests.Members;

[Collection(MongoDbFixture.CollectionName)]
public sealed class RegisterMemberTests(MongoDbFixture mongoDb) : IntegrationTestBase(mongoDb)
{
    [Fact]
    public async Task Given_a_new_email_When_registering_Then_the_member_is_created_with_a_lower_case_email()
    {
        // Act
        var response = await PostAsync<MemberResponse>("/members", new RegisterMemberRequest("Ada Lovelace", "Ada@Example.com"));

        // Assert
        CommonAssertions.StatusCodeMatches(response, HttpStatusCode.Created);
        Assert.Equal("ada@example.com", response.Content!.Email);
    }

    [Fact]
    public async Task Given_an_email_that_is_taken_in_another_case_When_registering_Then_conflict_is_returned()
    {
        // Arrange
        await PersistAsync(MemberBuilder.Simple().WithEmail("ada@example.com").Build());

        // Act
        var response = await PostAsync<MemberResponse>("/members", new RegisterMemberRequest("Ada L.", "ADA@example.com"));

        // Assert
        CommonAssertions.IsConflict(response, "A member with this email already exists");
    }
}
