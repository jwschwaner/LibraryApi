using LibraryApi.Application.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LibraryApi.Persistence;

/// <summary>
/// Hands query handlers read-only collections, so they have no way to insert, replace or delete.
/// </summary>
public sealed class ReadonlyLibraryDbContext(IMongoDatabase database) : IReadonlyApplicationDbContext
{
    public IReadonlyMongoCollection Authors => Readonly(CollectionNames.Authors);
    public IReadonlyMongoCollection Books => Readonly(CollectionNames.Books);
    public IReadonlyMongoCollection Members => Readonly(CollectionNames.Members);
    public IReadonlyMongoCollection Loans => Readonly(CollectionNames.Loans);

    private ReadonlyMongoCollection Readonly(string name) => new(database.GetCollection<BsonDocument>(name));
}
