using System.Net.Http.Json;
using System.Text.Json;
using LibraryApi.Application.Interfaces;
using LibraryApi.Domain.Authors;
using LibraryApi.Domain.Books;
using LibraryApi.Domain.Loans;
using LibraryApi.Domain.Members;
using LibraryApi.Tests.Common.Http;
using LibraryApi.Tests.Common.MongoDb;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryApi.IntegrationTests;

/// <summary>
/// xUnit builds a fresh instance per test, so <see cref="InitializeAsync"/> resets the database
/// and starts a fresh API host before each one. Starting the host also recreates the indexes the
/// reset dropped, so unique-index conflicts behave as in production.
/// Derived classes carry <c>[Collection(MongoDbFixture.CollectionName)]</c> themselves.
/// </summary>
public abstract class IntegrationTestBase(MongoDbFixture mongoDb) : IAsyncLifetime
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private LibraryApiFactory _factory = null!;
    private HttpClient _client = null!;

    protected MongoDbFixture MongoDb { get; } = mongoDb;

    public virtual async Task InitializeAsync()
    {
        await MongoDb.ResetAsync();

        _factory = new LibraryApiFactory(MongoDb.ConnectionString);
        _client = _factory.CreateClient();
    }

    public virtual async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    /// <summary>
    /// Writes entities through the API's own <see cref="IApplicationDbContext"/>, so they're stored
    /// with the real BSON mappings and collection names.
    /// </summary>
    protected async Task PersistAsync(params object[] entities)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        foreach (var entity in entities)
        {
            await (entity switch
            {
                Author author => db.Authors.InsertOneAsync(author),
                Book book => db.Books.InsertOneAsync(book),
                Member member => db.Members.InsertOneAsync(member),
                Loan loan => db.Loans.InsertOneAsync(loan),
                _ => throw new ArgumentException($"No collection for {entity.GetType().Name}", nameof(entities))
            });
        }
    }

    protected Task<ApiResponse<T>> GetAsync<T>(string path) =>
        SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, path));

    protected Task<ApiResponse<T>> PostAsync<T>(string path, object? body = null) =>
        SendAsync<T>(new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(body) });

    protected Task<ApiResponse<T>> PutAsync<T>(string path, object body) =>
        SendAsync<T>(new HttpRequestMessage(HttpMethod.Put, path) { Content = JsonContent.Create(body) });

    protected Task<ApiResponse<T>> DeleteAsync<T>(string path) =>
        SendAsync<T>(new HttpRequestMessage(HttpMethod.Delete, path));

    private async Task<ApiResponse<T>> SendAsync<T>(HttpRequestMessage request)
    {
        using var response = await _client.SendAsync(request);
        var rawBody = await response.Content.ReadAsStringAsync();

        return response.IsSuccessStatusCode
            ? new ApiResponse<T>(response.StatusCode, true, rawBody, Deserialize<T>(rawBody), null)
            : new ApiResponse<T>(response.StatusCode, false, rawBody, default, Deserialize<ErrorBody>(rawBody)?.Error);
    }

    private static T? Deserialize<T>(string rawBody) =>
        string.IsNullOrWhiteSpace(rawBody) ? default : JsonSerializer.Deserialize<T>(rawBody, Json);

    private sealed record ErrorBody(ApiError Error);
}
