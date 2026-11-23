using LibraryApi.Domain.Books;
using LibraryApi.Domain.Common;
using LibraryApi.Tests.Common;
using LibraryApi.Tests.Common.Builders;

namespace LibraryApi.UnitTests.Domain;

public sealed class BookTests
{
    public sealed class Create
    {
        [Fact]
        public void Given_padded_title_and_isbn_When_creating_a_book_Then_they_are_trimmed()
        {
            // Act
            var book = Book.Create("  The Hobbit ", " 9780547928227 ", 1937, Guid.NewGuid(), 3, TestConstants.NowUtc);

            // Assert
            Assert.Equal("The Hobbit", book.Title);
            Assert.Equal("9780547928227", book.Isbn);
            Assert.Equal(TestConstants.NowUtc, book.CreatedAtUtc);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Given_fewer_than_one_copy_When_creating_a_book_Then_it_is_refused(int totalCopies)
        {
            // Act
            var ex = Assert.Throws<DomainException>(() =>
                Book.Create("The Hobbit", "9780547928227", 1937, Guid.NewGuid(), totalCopies, TestConstants.NowUtc));

            // Assert
            Assert.Equal(BookErrors.InvalidCopyCount, ex.Code);
        }
    }

    public sealed class Update
    {
        private readonly Book _book = BookBuilder.Simple().By(AuthorBuilder.Simple().Build()).WithTotalCopies(3).Build();

        [Fact]
        public void Given_as_many_copies_as_are_on_loan_When_updating_Then_the_book_is_updated()
        {
            // Arrange
            var updatedAt = TestConstants.NowUtc.AddHours(1);

            // Act
            _book.Update(" New Title ", 2001, totalCopies: 2, activeLoans: 2, updatedAt);

            // Assert
            Assert.Equal("New Title", _book.Title);
            Assert.Equal(2001, _book.PublishedYear);
            Assert.Equal(2, _book.TotalCopies);
            Assert.Equal(updatedAt, _book.UpdatedAtUtc);
        }

        [Fact]
        public void Given_fewer_copies_than_are_on_loan_When_updating_Then_it_is_refused()
        {
            // Act
            var ex = Assert.Throws<DomainException>(() =>
                _book.Update("Title", 2000, totalCopies: 1, activeLoans: 2, TestConstants.NowUtc));

            // Assert
            Assert.Equal(BookErrors.CopiesOnLoan, ex.Code);
            Assert.Equal(3, _book.TotalCopies);
        }
    }
}
