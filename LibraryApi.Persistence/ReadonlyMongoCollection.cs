using LibraryApi.Application.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LibraryApi.Persistence;

internal sealed class ReadonlyMongoCollection(IMongoCollection<BsonDocument> collection) : IReadonlyMongoCollection
{
    private static readonly string[] WritingStages = ["$out", "$merge"];

    public async Task<List<BsonDocument>> AggregateAsync(List<BsonDocument> pipeline, CancellationToken cancellationToken)
    {
        if (pipeline.Any(stage => WritingStages.Any(stage.Contains)))
            throw new InvalidOperationException("Read-only pipelines can't use $out or $merge");

        return await collection.Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken);
    }
}
