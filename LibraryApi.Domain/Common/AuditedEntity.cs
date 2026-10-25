namespace LibraryApi.Domain.Common;

/// <summary>
/// Base entity with automatic creation and modification auditing.
/// The timestamps are set by the persistence layer, never by domain code.
/// </summary>
public abstract class AuditedEntity : BaseEntity
{
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
