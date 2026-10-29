using LibraryApi.Domain.Authors;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Loans;
using LibraryApi.Domain.Members;
using MongoDB.Driver;

namespace LibraryApi.Application.Interfaces;

/// <summary>
/// The write side: command handlers load entities, call domain methods and write each changed
/// document back. MongoDB has no change tracker, so every insert, replace and delete is explicit.
/// </summary>
public interface IApplicationDbContext
{
    IMongoCollection<Author> Authors { get; }
    IMongoCollection<Book> Books { get; }
    IMongoCollection<Member> Members { get; }
    IMongoCollection<Loan> Loans { get; }
}
