using LibraryApi.Application.Interfaces;
using MongoDB.Driver;

namespace LibraryApi.Persistence;

internal sealed class ReadonlyMongoCollection<TDocument>(IMongoCollection<TDocument> collection)
    : IReadonlyMongoCollection<TDocument>
{
    private static readonly string[] WritingStages = ["$out", "$merge"];

    public async Task<List<TResult>> AggregateAsync<TResult>(
        PipelineDefinition<TDocument, TResult> pipeline,
        CancellationToken cancellationToken)
    {
        if (pipeline.Stages.Any(stage => WritingStages.Contains(stage.OperatorName)))
            throw new InvalidOperationException("Read-only pipelines can't use $out or $merge");

        return await collection.Aggregate(pipeline, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken);
    }
}
