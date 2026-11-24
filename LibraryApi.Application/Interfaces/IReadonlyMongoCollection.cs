using MongoDB.Driver;

namespace LibraryApi.Application.Interfaces;

/// <summary>
/// The read half of a MongoDB collection: query handlers run typed aggregation pipelines built
/// with the driver's builders and get the projected results back. A pipeline with a writing
/// stage (<c>$out</c>, <c>$merge</c>) is rejected.
/// </summary>
public interface IReadonlyMongoCollection<TDocument>
{
    Task<List<TResult>> AggregateAsync<TResult>(
        PipelineDefinition<TDocument, TResult> pipeline,
        CancellationToken cancellationToken);
}
