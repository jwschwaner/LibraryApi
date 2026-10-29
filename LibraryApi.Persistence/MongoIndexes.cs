using LibraryApi.Domain.Books;
using LibraryApi.Domain.Loans;
using LibraryApi.Domain.Members;
using MongoDB.Driver;

namespace LibraryApi.Persistence;

/// <summary>
/// MongoDB has no migrations: collections appear on first write. Indexes are the only schema,
/// and creating an index that already exists is a no-op, so this runs on every startup.
/// </summary>
internal static class MongoIndexes
{
    public static async Task EnsureAsync(LibraryDbContext db, CancellationToken cancellationToken)
    {
        await db.Books.Indexes.CreateOneAsync(
            new CreateIndexModel<Book>(
                Builders<Book>.IndexKeys.Ascending(b => b.Isbn),
                new CreateIndexOptions { Unique = true }),
            cancellationToken: cancellationToken);

        await db.Members.Indexes.CreateOneAsync(
            new CreateIndexModel<Member>(
                Builders<Member>.IndexKeys.Ascending(m => m.Email),
                new CreateIndexOptions { Unique = true }),
            cancellationToken: cancellationToken);

        // Lending rules count active loans per book and per member
        await db.Loans.Indexes.CreateManyAsync(
            [
                new CreateIndexModel<Loan>(
                    Builders<Loan>.IndexKeys.Ascending(l => l.BookId).Ascending(l => l.ReturnedAtUtc)),
                new CreateIndexModel<Loan>(
                    Builders<Loan>.IndexKeys.Ascending(l => l.MemberId).Ascending(l => l.ReturnedAtUtc))
            ],
            cancellationToken);
    }
}
