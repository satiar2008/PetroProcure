using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Orders;

public sealed class MaterialNeed : AuditableEntity<Guid>
{
    private MaterialNeed() : base(Guid.Empty) { }

    public MaterialNeed(Guid id, string needNumber, Guid mescItemId, string mescCode, string mescGeneralGroupCode,
        string generalDescription, string specificDescription, Guid unitOfMeasureId, decimal requestedQuantity,
        DateOnly? neededByDate, Guid sourceDepartmentId, Guid? applicantDepartmentId, Guid requestedByUserId,
        MaterialNeedPriority priority, string? description = null) : base(id)
    {
        if (mescItemId == Guid.Empty) throw new ArgumentException("MESC item is required.", nameof(mescItemId));
        if (requestedQuantity <= 0) throw new ArgumentOutOfRangeException(nameof(requestedQuantity));
        NeedNumber = Required(needNumber, nameof(needNumber));
        MescItemId = mescItemId;
        MescCode = Required(mescCode, nameof(mescCode));
        MescGeneralGroupCode = Required(mescGeneralGroupCode, nameof(mescGeneralGroupCode));
        GeneralDescription = Required(generalDescription, nameof(generalDescription));
        SpecificDescription = Required(specificDescription, nameof(specificDescription));
        UnitOfMeasureId = unitOfMeasureId;
        RequestedQuantity = requestedQuantity;
        NeededByDate = neededByDate;
        SourceDepartmentId = sourceDepartmentId;
        ApplicantDepartmentId = applicantDepartmentId;
        RequestedByUserId = requestedByUserId;
        Priority = priority;
        Description = description?.Trim();
        Status = MaterialNeedStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public string NeedNumber { get; private set; } = string.Empty;
    public Guid MescItemId { get; private set; }
    public string MescCode { get; private set; } = string.Empty;
    public string MescGeneralGroupCode { get; private set; } = string.Empty;
    public string GeneralDescription { get; private set; } = string.Empty;
    public string SpecificDescription { get; private set; } = string.Empty;
    public Guid UnitOfMeasureId { get; private set; }
    public decimal RequestedQuantity { get; private set; }
    public DateOnly? NeededByDate { get; private set; }
    public Guid SourceDepartmentId { get; private set; }
    public Guid? ApplicantDepartmentId { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public MaterialNeedStatus Status { get; private set; }
    public MaterialNeedPriority Priority { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public Guid? RelatedIndentId { get; private set; }
    public string? RejectionReason { get; private set; }

    public void Submit()
    {
        EnsureStatus(MaterialNeedStatus.Draft);
        Status = MaterialNeedStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
    }

    public void Review(Guid reviewedByUserId)
    {
        EnsureStatus(MaterialNeedStatus.Submitted);
        Status = MaterialNeedStatus.UnderReview;
        ReviewedByUserId = reviewedByUserId;
        ReviewedAt = DateTime.UtcNow;
    }

    public void Approve(Guid reviewedByUserId)
    {
        if (Status is not (MaterialNeedStatus.Submitted or MaterialNeedStatus.UnderReview))
            throw new InvalidOperationException("Material need must be submitted or under review before approval.");
        Status = MaterialNeedStatus.Approved;
        ReviewedByUserId = reviewedByUserId;
        ReviewedAt = DateTime.UtcNow;
        RejectionReason = null;
    }

    public void Reject(Guid reviewedByUserId, string reason)
    {
        if (Status is not (MaterialNeedStatus.Submitted or MaterialNeedStatus.UnderReview))
            throw new InvalidOperationException("Material need must be submitted or under review before rejection.");
        Status = MaterialNeedStatus.Rejected;
        ReviewedByUserId = reviewedByUserId;
        ReviewedAt = DateTime.UtcNow;
        RejectionReason = Required(reason, nameof(reason));
    }

    public void MarkConvertedToIndent(Guid indentId)
    {
        EnsureStatus(MaterialNeedStatus.Approved);
        RelatedIndentId = indentId;
        Status = MaterialNeedStatus.ConvertedToIndent;
    }

    private void EnsureStatus(MaterialNeedStatus expected)
    {
        if (Status != expected) throw new InvalidOperationException($"Material need must be {expected} for this operation.");
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}
