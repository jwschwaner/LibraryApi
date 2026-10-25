using LibraryApi.Domain.Authors;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Persistence.Seeding;

/// <summary>Fills an empty database with a few authors, books and members for demos.</summary>
public static class DemoDataSeeder
{
    public static async Task SeedAsync(LibraryDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Authors.AnyAsync(cancellationToken))
            return;

        var tolkien = Author.Create("J.R.R.", "Tolkien", 1892);
        var leGuin = Author.Create("Ursula K.", "Le Guin", 1929);
        var blixen = Author.Create("Karen", "Blixen", 1885);
        db.Authors.AddRange(tolkien, leGuin, blixen);

        db.Books.AddRange(
            Book.Create("The Hobbit", "9780547928227", 1937, tolkien.Id, totalCopies: 3),
            Book.Create("The Fellowship of the Ring", "9780547928210", 1954, tolkien.Id, totalCopies: 2),
            Book.Create("A Wizard of Earthsea", "9780547773742", 1968, leGuin.Id, totalCopies: 1),
            Book.Create("The Left Hand of Darkness", "9780441478125", 1969, leGuin.Id, totalCopies: 2),
            Book.Create("Out of Africa", "9780679600213", 1937, blixen.Id, totalCopies: 1));

        db.Members.AddRange(
            Member.Create("Ada Lovelace", "ada@example.com"),
            Member.Create("Alan Turing", "alan@example.com"));

        await db.SaveChangesAsync(cancellationToken);
    }
}
