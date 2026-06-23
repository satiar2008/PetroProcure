using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.PurchaseFiles;

public sealed class PurchaseFileStatusHistory : Entity<Guid>
{
    public PurchaseFileStatusHistory(
        Guid id, Guid purchaseFileId, PurchaseFileStatus fromStatus,
        PurchaseFileStatus toStatus, Guid changedByUserId, string? reason, Guid? departmentId)
        : base(id)
    {
        PurchaseFileId = purchaseFileId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ChangedByUserId = changedByUserId;
        ChangedAt = DateTime.UtcNow;
        Reason = reason?.Trim();
        DepartmentId = departmentId;
    }

    public Guid PurchaseFileId { get; private set; }
    public PurchaseFileStatus FromStatus { get; private set; }
    public PurchaseFileStatus ToStatus { get; private set; }
    public Guid ChangedByUserId { get; private set; }
    public DateTime ChangedAt { get; private set; }
    public string? Reason { get; private set; }
    public Guid? DepartmentId { get; private set; }
}
