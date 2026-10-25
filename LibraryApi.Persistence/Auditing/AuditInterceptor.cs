using LibraryApi.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LibraryApi.Persistence.Auditing;

public sealed class AuditInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var ctx = eventData.Context;
        if (ctx is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var now = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var entry in ctx.ChangeTracker.Entries<AuditedEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Property(p => p.CreatedAtUtc).CurrentValue = now;
            else if (entry.State == EntityState.Modified)
                entry.Property(p => p.UpdatedAtUtc).CurrentValue = now;
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
