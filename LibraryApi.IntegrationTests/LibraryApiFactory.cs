using LibraryApi.Tests.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;

namespace LibraryApi.IntegrationTests;

/// <summary>
/// Boots the real API against the test container, with the clock frozen at
/// <see cref="TestConstants.NowUtc"/>.
/// </summary>
public sealed class LibraryApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    public FakeTimeProvider Clock { get; } = new(new DateTimeOffset(TestConstants.NowUtc));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Anything but Development, so Program doesn't seed the demo data
        builder.UseEnvironment("IntegrationTest");

        // A host setting rather than app configuration: Program reads the connection string
        // while registering services, before later configuration sources are applied
        builder.UseSetting("ConnectionStrings:MongoDb", connectionString);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }
}
