using LibraryApi.Domain.Members;
using LibraryApi.Tests.Common;

namespace LibraryApi.UnitTests.Domain;

public sealed class MemberTests
{
    public sealed class Create
    {
        [Fact]
        public void Given_a_mixed_case_padded_email_When_creating_a_member_Then_it_is_trimmed_and_lower_cased()
        {
            // Act
            var member = Member.Create(" Ada Lovelace ", " Ada@Example.COM ", TestConstants.NowUtc);

            // Assert
            Assert.Equal("Ada Lovelace", member.FullName);
            Assert.Equal("ada@example.com", member.Email);
        }
    }
}
