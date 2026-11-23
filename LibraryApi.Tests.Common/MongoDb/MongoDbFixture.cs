using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace LibraryApi.Tests.Common.MongoDb;

/// <summary>
/// Owns the MongoDB container for a whole test run. Starting one costs seconds, so tests share a
/// single container through a collection fixture and reset the database between cases instead.
/// </summary>
public sealed class MongoDbFixture : IAsyncLifetime
{
    public const string DatabaseName = "library_integration_tests";

    /// <summary>Shared by every [CollectionDefinition] built on this fixture.</summary>
    public const string CollectionName = "MongoDb";

    private readonly MongoDbContainer _container = new MongoDbBuilder("mongo:7.0").Build();

    private IMongoClient _client = null!;

    /// <summary>
    /// The API reads the database from the connection string, so it's added here. The container's
    /// user lives in the admin database, which has to stay the auth source once a database is set.
    /// </summary>
    public string ConnectionString => new MongoUrlBuilder(_container.GetConnectionString())
    {
        DatabaseName = DatabaseName,
        AuthenticationSource = "admin"
    }.ToString();

    public IMongoDatabase Database => _client.GetDatabase(DatabaseName);

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _client = new MongoClient(ConnectionString);
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    /// <summary>Drops every collection so one test's data can't leak into the next.</summary>
    public async Task ResetAsync()
    {
        var names = await (await Database.ListCollectionNamesAsync()).ToListAsync();
        foreach (var name in names)
            await Database.DropCollectionAsync(name);
    }
}
