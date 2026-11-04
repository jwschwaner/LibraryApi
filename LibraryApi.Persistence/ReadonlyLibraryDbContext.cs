using LibraryApi.Application.Interfaces;
using LibraryApi.Domain.Authors;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Loans;
using LibraryApi.Domain.Members;
using MongoDB.Driver;

namespace LibraryApi.Persistence;

/// <summary>
/// Hands query handlers LINQ queryables only, so they have no way to insert, replace or delete.
/// </summary>
public sealed class ReadonlyLibraryDbContext(IMongoDatabase database) : IReadonlyApplicationDbContext
{
    public IQueryable<Author> Authors => database.GetCollection<Author>(CollectionNames.Authors).AsQueryable();
    public IQueryable<Book> Books => database.GetCollection<Book>(CollectionNames.Books).AsQueryable();
    public IQueryable<Member> Members => database.GetCollection<Member>(CollectionNames.Members).AsQueryable();
    public IQueryable<Loan> Loans => database.GetCollection<Loan>(CollectionNames.Loans).AsQueryable();
}
