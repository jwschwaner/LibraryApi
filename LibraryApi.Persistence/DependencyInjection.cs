using LibraryApi.Application.Interfaces;
using LibraryApi.Persistence.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryApi.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration cfg)
    {
        return services
                .AddSingleton<AuditInterceptor>()
                .AddDbContext<LibraryDbContext>((sp, opts) =>
                {
                    opts.UseSqlite(cfg.GetConnectionString("DefaultConnection"));
                    opts.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
                })
                // Commands get the tracking context, queries a separate no-tracking instance
                .AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<LibraryDbContext>())
                .AddScoped<IReadonlyApplicationDbContext>(provider =>
                {
                    var options = provider.GetRequiredService<DbContextOptions<LibraryDbContext>>();
                    return new ReadonlyLibraryDbContext(options);
                })
            ;
    }
}
