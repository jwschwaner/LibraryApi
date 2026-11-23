using LibraryApi.Application.Books;
using LibraryApi.Domain.Authors;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Members;
using LibraryApi.Tests.Common.Builders;
using LibraryApi.Tests.Common.Http;
using LibraryApi.Tests.Common.MongoDb;

namespace LibraryApi.IntegrationTests.Books;

[Collection(MongoDbFixture.CollectionName)]
public sealed class SearchBooksTests(MongoDbFixture mongoDb) : IntegrationTestBase(mongoDb)
{
    private readonly Author _tolkien = AuthorBuilder.Simple().WithFirstName("J.R.R.").WithLastName("Tolkien").Build();
    private readonly Author _leGuin = AuthorBuilder.Typical().Build();
    private readonly Member _member = MemberBuilder.Simple().Build();
    private Book _hobbit = null!;
    private Book _earthsea = null!;
    private Book _regexTitle = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _hobbit = BookBuilder.Simple().By(_tolkien).WithTitle("The Hobbit").Build();
        _earthsea = BookBuilder.Simple().By(_leGuin).WithTitle("A Wizard of Earthsea").Build();
        _regexTitle = BookBuilder.Simple().By(_leGuin).WithTitle("Notes (1.0) on Writing").Build();
        await PersistAsync(_tolkien, _leGuin, _member, _hobbit, _earthsea, _regexTitle);
    }

    [Fact]
    public async Task Given_no_search_When_searching_books_Then_every_book_is_returned_sorted_by_title()
    {
        // Act
        var response = await GetAsync<List<BookResponse>>("/books");

        // Assert
        CommonAssertions.IsSuccessStatusCode(response);
        Assert.Equal(
            ["A Wizard of Earthsea", "Notes (1.0) on Writing", "The Hobbit"],
            response.Content!.Select(b => b.Title));
    }

    [Fact]
    public async Task Given_part_of_a_title_in_another_case_When_searching_books_Then_the_book_is_found()
    {
        // Act
        var response = await GetAsync<List<BookResponse>>("/books?search=HOBB");

        // Assert
        CommonAssertions.IsSuccessStatusCode(response);
        Assert.Equal(_hobbit.Id, Assert.Single(response.Content!).Id);
    }

    [Fact]
    public async Task Given_an_author_name_When_searching_books_Then_the_authors_books_are_found()
    {
        // Act
        var response = await GetAsync<List<BookResponse>>("/books?search=guin");

        // Assert
        CommonAssertions.IsSuccessStatusCode(response);
        Assert.Equal([_earthsea.Id, _regexTitle.Id], response.Content!.Select(b => b.Id));
    }

    [Fact]
    public async Task Given_regex_characters_When_searching_books_Then_they_are_matched_literally()
    {
        // Act
        var response = await GetAsync<List<BookResponse>>($"/books?search={Uri.EscapeDataString("(1.0)")}");

        // Assert
        CommonAssertions.IsSuccessStatusCode(response);
        Assert.Equal(_regexTitle.Id, Assert.Single(response.Content!).Id);
    }

    [Fact]
    public async Task Given_a_book_with_every_copy_on_loan_When_searching_available_books_Then_it_is_left_out()
    {
        // Arrange
        await PersistAsync(LoanBuilder.Of(_hobbit, _member).Build());

        // Act
        var response = await GetAsync<List<BookResponse>>("/books?availableOnly=true");

        // Assert
        CommonAssertions.IsSuccessStatusCode(response);
        Assert.DoesNotContain(response.Content!, b => b.Id == _hobbit.Id);
        Assert.Equal(2, response.Content!.Count);
    }
}
