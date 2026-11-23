using MongoDB.Bson;

namespace LibraryApi.Application.Interfaces;

/// <summary>
/// The read half of a MongoDB collection: query handlers run aggregation pipelines written as
/// <see cref="BsonDocument"/> stages and get raw documents back. A pipeline with a writing
/// stage (<c>$out</c>, <c>$merge</c>) is rejected.
/// </summary>
public interface IReadonlyMongoCollection
{
    Task<List<BsonDocument>> AggregateAsync(List<BsonDocument> pipeline, CancellationToken cancellationToken);
}
