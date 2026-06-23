namespace PetroProcure.Domain.Common;

public abstract class AuditableEntity<TId> : Entity<TId>
    where TId : notnull
{
    protected AuditableEntity(TId id)
        : base(id)
    {
    }

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public string? CreatedBy { get; private set; }

    public DateTime? ModifiedAtUtc { get; private set; }

    public string? ModifiedBy { get; private set; }

    public void MarkCreated(string? userId, DateTime? createdAtUtc = null)
    {
        CreatedBy = userId;
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }

    public void MarkModified(string? userId, DateTime? modifiedAtUtc = null)
    {
        ModifiedBy = userId;
        ModifiedAtUtc = modifiedAtUtc ?? DateTime.UtcNow;
    }
}
