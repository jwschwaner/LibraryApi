using LibraryApi.Tests.Common.MongoDb;

namespace LibraryApi.IntegrationTests;

/// <summary>
/// xUnit discovers collection definitions per test assembly, so this has to be declared here
/// rather than next to the fixture in LibraryApi.Tests.Common. Every class in the collection
/// shares one container and runs one at a time.
/// </summary>
[CollectionDefinition(MongoDbFixture.CollectionName)]
public sealed class MongoDbTestCollection : ICollectionFixture<MongoDbFixture>;
