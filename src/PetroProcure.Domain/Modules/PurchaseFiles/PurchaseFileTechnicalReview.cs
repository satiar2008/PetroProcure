using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.PurchaseFiles;

public sealed class PurchaseFileTechnicalReview : AuditableEntity<Guid>
{
    private PurchaseFileTechnicalReview() : base(Guid.Empty)
    {
        RequestComment = null;
    }

    public PurchaseFileTechnicalReview(Guid id, Guid purchaseFileId, Guid departmentId, Guid requestedByUserId,
        string? requestComment, DateTime? requestedAt = null) : base(id)
    {
        PurchaseFileId = purchaseFileId;
        DepartmentId = departmentId;
        RequestedByUserId = requestedByUserId;
        RequestComment = Trim(requestComment);
        Status = PurchaseFileTechnicalReviewStatus.Requested;
        RequestedAt = requestedAt ?? DateTime.UtcNow;
    }

    public Guid PurchaseFileId { get; private set; }
    public Guid DepartmentId { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public PurchaseFileTechnicalReviewStatus Status { get; private set; }
    public PurchaseFileTechnicalReviewDecision? Decision { get; private set; }
    public string? RequestComment { get; private set; }
    public string? Comments { get; private set; }
    public string? RecommendationNotes { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid? WorkflowInstanceId { get; private set; }
    public Guid? ApplicantInboxTaskId { get; private set; }
    public Guid? ReturnInboxTaskId { get; private set; }

    public void LinkApplicantTask(Guid workflowInstanceId, Guid inboxTaskId)
    {
        WorkflowInstanceId = workflowInstanceId;
        ApplicantInboxTaskId = inboxTaskId;
    }

    public void LinkReturnTask(Guid inboxTaskId) => ReturnInboxTaskId = inboxTaskId;

    public void Start(Guid userId)
    {
        if (Status != PurchaseFileTechnicalReviewStatus.Requested)
            throw new InvalidOperationException("Only requested reviews can be started.");
        ReviewedByUserId = userId;
        Status = PurchaseFileTechnicalReviewStatus.InReview;
        StartedAt = DateTime.UtcNow;
    }

    public void Submit(Guid userId, PurchaseFileTechnicalReviewDecision decision, string comments, string? recommendationNotes)
    {
        if (Status is PurchaseFileTechnicalReviewStatus.Approved or PurchaseFileTechnicalReviewStatus.Rejected
            or PurchaseFileTechnicalReviewStatus.Cancelled)
            throw new InvalidOperationException("Completed reviews cannot be submitted again.");
        if (decision == PurchaseFileTechnicalReviewDecision.NeedsClarification)
        {
            RequestClarification(userId, comments, recommendationNotes);
            return;
        }

        ReviewedByUserId = userId;
        Decision = decision;
        Comments = Required(comments);
        RecommendationNotes = Trim(recommendationNotes);
        Status = decision == PurchaseFileTechnicalReviewDecision.TechnicallyRejected
            ? PurchaseFileTechnicalReviewStatus.Rejected
            : PurchaseFileTechnicalReviewStatus.Approved;
        CompletedAt = DateTime.UtcNow;
    }

    public void RequestClarification(Guid userId, string comments, string? recommendationNotes)
    {
        if (Status is PurchaseFileTechnicalReviewStatus.Approved or PurchaseFileTechnicalReviewStatus.Rejected
            or PurchaseFileTechnicalReviewStatus.Cancelled)
            throw new InvalidOperationException("Completed reviews cannot request clarification.");
        ReviewedByUserId = userId;
        Decision = PurchaseFileTechnicalReviewDecision.NeedsClarification;
        Comments = Required(comments);
        RecommendationNotes = Trim(recommendationNotes);
        Status = PurchaseFileTechnicalReviewStatus.ClarificationRequested;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel(Guid userId, string? comments)
    {
        if (Status is PurchaseFileTechnicalReviewStatus.Approved or PurchaseFileTechnicalReviewStatus.Rejected
            or PurchaseFileTechnicalReviewStatus.Cancelled)
            return;
        ReviewedByUserId = userId;
        Comments = Trim(comments);
        Status = PurchaseFileTechnicalReviewStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }

    private static string Required(string value) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Comments are required.") : value.Trim();

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
