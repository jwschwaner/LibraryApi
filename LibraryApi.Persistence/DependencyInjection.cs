using LibraryApi.Application.Interfaces;
using LibraryApi.Persistence.Mapping;
using LibraryApi.Persistence.Seeding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace LibraryApi.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration cfg)
    {
        MongoMappings.Register();

        // The connection string names the database, e.g. mongodb://localhost:27017/library
        var url = MongoUrl.Create(cfg.GetConnectionString("MongoDb")
            ?? throw new InvalidOperationException("ConnectionStrings:MongoDb is not configured"));
        if (string.IsNullOrEmpty(url.DatabaseName))
            throw new InvalidOperationException("ConnectionStrings:MongoDb must include a database name");

        return services
                // The client is thread-safe and owns the connection pool: one per process
                .AddSingleton<IMongoClient>(_ => new MongoClient(url))
                .AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(url.DatabaseName))
                // Commands get the collections, queries only LINQ queryables over them
                .AddScoped<LibraryDbContext>()
                .AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<LibraryDbContext>())
                .AddScoped<IReadonlyApplicationDbContext, ReadonlyLibraryDbContext>()
            ;
    }

    /// <summary>Creates indexes and, when asked, seeds demo data into an empty database.</summary>
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services,
        bool seedDemoData,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        await MongoIndexes.EnsureAsync(db, cancellationToken);

        if (seedDemoData)
        {
            var nowUtc = scope.ServiceProvider.GetRequiredService<TimeProvider>().GetUtcNow().UtcDateTime;
            await DemoDataSeeder.SeedAsync(db, nowUtc, cancellationToken);
        }
    }
}
