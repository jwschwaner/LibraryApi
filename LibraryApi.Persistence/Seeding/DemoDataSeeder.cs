using LibraryApi.Domain.Authors;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Members;
using MongoDB.Driver;

namespace LibraryApi.Persistence.Seeding;

/// <summary>Fills an empty database with a few authors, books and members for demos.</summary>
internal static class DemoDataSeeder
{
    public static async Task SeedAsync(LibraryDbContext db, DateTime nowUtc, CancellationToken cancellationToken)
    {
        if (await db.Authors.Find(FilterDefinition<Author>.Empty).AnyAsync(cancellationToken))
            return;

        var tolkien = Author.Create("J.R.R.", "Tolkien", 1892, nowUtc);
        var leGuin = Author.Create("Ursula K.", "Le Guin", 1929, nowUtc);
        var blixen = Author.Create("Karen", "Blixen", 1885, nowUtc);
        await db.Authors.InsertManyAsync([tolkien, leGuin, blixen], cancellationToken: cancellationToken);

        await db.Books.InsertManyAsync(
            [
                Book.Create("The Hobbit", "9780547928227", 1937, tolkien.Id, totalCopies: 3, nowUtc),
                Book.Create("The Fellowship of the Ring", "9780547928210", 1954, tolkien.Id, totalCopies: 2, nowUtc),
                Book.Create("A Wizard of Earthsea", "9780547773742", 1968, leGuin.Id, totalCopies: 1, nowUtc),
                Book.Create("The Left Hand of Darkness", "9780441478125", 1969, leGuin.Id, totalCopies: 2, nowUtc),
                Book.Create("Out of Africa", "9780679600213", 1937, blixen.Id, totalCopies: 1, nowUtc)
            ],
            cancellationToken: cancellationToken);

        await db.Members.InsertManyAsync(
            [
                Member.Create("Ada Lovelace", "ada@example.com", nowUtc),
                Member.Create("Alan Turing", "alan@example.com", nowUtc)
            ],
            cancellationToken: cancellationToken);
    }
}
