namespace LibraryApi.Application.Interfaces;

/// <summary>
/// The read side: query handlers get read-only collections they can only run aggregation
/// pipelines on, so a query can't change state even by accident.
/// </summary>
public interface IReadonlyApplicationDbContext
{
    IReadonlyMongoCollection Authors { get; }
    IReadonlyMongoCollection Books { get; }
    IReadonlyMongoCollection Members { get; }
    IReadonlyMongoCollection Loans { get; }
}
