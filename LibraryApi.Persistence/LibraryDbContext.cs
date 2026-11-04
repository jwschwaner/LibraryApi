using LibraryApi.Application.Interfaces;
using LibraryApi.Domain.Authors;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Loans;
using LibraryApi.Domain.Members;
using MongoDB.Driver;

namespace LibraryApi.Persistence;

public sealed class LibraryDbContext(IMongoDatabase database) : IApplicationDbContext
{
    public IMongoCollection<Author> Authors => database.GetCollection<Author>(CollectionNames.Authors);
    public IMongoCollection<Book> Books => database.GetCollection<Book>(CollectionNames.Books);
    public IMongoCollection<Member> Members => database.GetCollection<Member>(CollectionNames.Members);
    public IMongoCollection<Loan> Loans => database.GetCollection<Loan>(CollectionNames.Loans);
}
