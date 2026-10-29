namespace LibraryApi.Domain.Common;

/// <summary>
/// Base entity with creation and modification timestamps. MongoDB has no save interceptor to
/// fill them in, so factories and behavior methods take the current time and call these.
/// </summary>
public abstract class AuditedEntity : BaseEntity
{
    public DateTime CreatedAtUtc { get; protected set; }
    public DateTime? UpdatedAtUtc { get; protected set; }

    protected void MarkCreated(DateTime nowUtc) => CreatedAtUtc = nowUtc;

    protected void MarkUpdated(DateTime nowUtc) => UpdatedAtUtc = nowUtc;
}
