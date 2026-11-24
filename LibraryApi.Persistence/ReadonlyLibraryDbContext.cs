using LibraryApi.Application.Interfaces;
using LibraryApi.Domain.Authors;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Loans;
using LibraryApi.Domain.Members;
using MongoDB.Driver;

namespace LibraryApi.Persistence;

/// <summary>
/// Hands query handlers read-only collections, so they have no way to insert, replace or delete.
/// </summary>
public sealed class ReadonlyLibraryDbContext(IMongoDatabase database) : IReadonlyApplicationDbContext
{
    public IReadonlyMongoCollection<Author> Authors => Readonly<Author>(CollectionNames.Authors);
    public IReadonlyMongoCollection<Book> Books => Readonly<Book>(CollectionNames.Books);
    public IReadonlyMongoCollection<Member> Members => Readonly<Member>(CollectionNames.Members);
    public IReadonlyMongoCollection<Loan> Loans => Readonly<Loan>(CollectionNames.Loans);

    private ReadonlyMongoCollection<T> Readonly<T>(string name) => new(database.GetCollection<T>(name));
}
