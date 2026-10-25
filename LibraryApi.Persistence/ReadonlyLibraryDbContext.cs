using LibraryApi.Application.Interfaces;
using LibraryApi.Domain.Authors;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Loans;
using LibraryApi.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Persistence;

public sealed class ReadonlyLibraryDbContext : LibraryDbContext, IReadonlyApplicationDbContext
{
    public ReadonlyLibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    IQueryable<Author> IReadonlyApplicationDbContext.Authors => Authors;
    IQueryable<Book> IReadonlyApplicationDbContext.Books => Books;
    IQueryable<Member> IReadonlyApplicationDbContext.Members => Members;
    IQueryable<Loan> IReadonlyApplicationDbContext.Loans => Loans;

    public override int SaveChanges()
        => throw new NotSupportedException("Readonly context: SaveChanges is not allowed.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Readonly context: SaveChangesAsync is not allowed.");
}
