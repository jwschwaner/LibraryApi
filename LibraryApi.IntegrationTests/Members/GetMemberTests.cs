using System.Net;
using LibraryApi.Application.Members.GetMember;
using LibraryApi.Tests.Common;
using LibraryApi.Tests.Common.Builders;
using LibraryApi.Tests.Common.Http;
using LibraryApi.Tests.Common.MongoDb;

namespace LibraryApi.IntegrationTests.Members;

[Collection(MongoDbFixture.CollectionName)]
public sealed class GetMemberTests(MongoDbFixture mongoDb) : IntegrationTestBase(mongoDb)
{
    [Fact]
    public async Task Given_active_returned_and_overdue_loans_When_requesting_the_member_Then_only_active_loans_are_listed_by_due_date()
    {
        // Arrange
        var author = AuthorBuilder.Simple().Build();
        var hobbit = BookBuilder.Simple().By(author).WithTitle("The Hobbit").Build();
        var earthsea = BookBuilder.Simple().By(author).WithTitle("A Wizard of Earthsea").Build();
        var member = MemberBuilder.Typical().Build();
        var active = LoanBuilder.Of(hobbit, member).Build();
        var overdue = LoanBuilder.Of(earthsea, member).Overdue(daysOverdue: 3).Build();
        await PersistAsync(
            author, hobbit, earthsea, member, active, overdue, LoanBuilder.Of(hobbit, member).Returned().Build());

        // Act
        var response = await GetAsync<MemberDetailsResponse>($"/members/{member.Id}");

        // Assert
        CommonAssertions.IsSuccessStatusCode(response);
        var content = response.Content!;
        Assert.Equal("Ada Lovelace", content.FullName);
        Assert.Equal(
            [
                new MemberLoanResponse(overdue.Id, earthsea.Id, "A Wizard of Earthsea", TestConstants.Today.AddDays(-3), true),
                new MemberLoanResponse(active.Id, hobbit.Id, "The Hobbit", TestConstants.Today.AddDays(14), false)
            ],
            content.ActiveLoans);
    }

    [Fact]
    public async Task Given_an_unknown_member_id_When_requesting_the_member_Then_not_found_is_returned()
    {
        // Act
        var response = await GetAsync<MemberDetailsResponse>($"/members/{TestConstants.UnknownId}");

        // Assert
        CommonAssertions.StatusCodeMatches(response, HttpStatusCode.NotFound);
    }
}
