using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.TenderCommission;

public sealed class TenderCommissionSession : AuditableEntity<Guid>
{
    private readonly List<TenderCommissionMember> _members = [];
    private readonly List<TenderCommissionAgendaItem> _agendaItems = [];
    private readonly List<TenderCommissionMinute> _minutes = [];
    private readonly List<TenderCommissionDecision> _decisions = [];
    private readonly List<TenderCommissionAttachment> _attachments = [];

    private TenderCommissionSession() : base(Guid.Empty)
    {
        SessionNumber = string.Empty;
        Title = string.Empty;
    }

    public TenderCommissionSession(Guid id, string sessionNumber, Guid tenderId, Guid purchaseFileId,
        string title, DateTime sessionDate, string? location, string? description, Guid createdByUserId,
        DateTime? createdAt = null) : base(id)
    {
        SessionNumber = Required(sessionNumber, nameof(sessionNumber));
        TenderId = tenderId;
        PurchaseFileId = purchaseFileId;
        Title = Required(title, nameof(title));
        Description = Trim(description);
        SessionDate = sessionDate;
        Location = Trim(location);
        Status = TenderCommissionSessionStatus.Draft;
        CreatedAt = createdAt ?? DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
    }

    public string SessionNumber { get; private set; }
    public Guid TenderId { get; private set; }
    public Guid PurchaseFileId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTime SessionDate { get; private set; }
    public string? Location { get; private set; }
    public TenderCommissionSessionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public Guid? StartedByUserId { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid? CompletedByUserId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public string? CancellationReason { get; private set; }

    public IReadOnlyCollection<TenderCommissionMember> Members => _members.AsReadOnly();
    public IReadOnlyCollection<TenderCommissionAgendaItem> AgendaItems => _agendaItems.AsReadOnly();
    public IReadOnlyCollection<TenderCommissionMinute> Minutes => _minutes.AsReadOnly();
    public IReadOnlyCollection<TenderCommissionDecision> Decisions => _decisions.AsReadOnly();
    public IReadOnlyCollection<TenderCommissionAttachment> Attachments => _attachments.AsReadOnly();

    public void Update(string title, DateTime sessionDate, string? location, string? description)
    {
        EnsureEditable();
        Title = Required(title, nameof(title));
        SessionDate = sessionDate;
        Location = Trim(location);
        Description = Trim(description);
    }

    public void Schedule(DateTime sessionDate, string? location)
    {
        EnsureEditable();
        SessionDate = sessionDate;
        Location = Trim(location);
        Status = TenderCommissionSessionStatus.Scheduled;
    }

    public void Start(Guid userId)
    {
        EnsureNotFinal();
        if (Status is TenderCommissionSessionStatus.Completed or TenderCommissionSessionStatus.Approved)
            throw new InvalidOperationException("Completed or approved commission sessions are read-only.");
        Status = TenderCommissionSessionStatus.InProgress;
        StartedAt ??= DateTime.UtcNow;
        StartedByUserId ??= userId;
    }

    public void Complete(Guid userId)
    {
        EnsureNotFinal();
        if (Status != TenderCommissionSessionStatus.InProgress)
            throw new InvalidOperationException("Commission session must be in progress before completion.");
        Status = TenderCommissionSessionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        CompletedByUserId = userId;
    }

    public void Approve(Guid userId)
    {
        EnsureNotFinal();
        if (_members.Count == 0) throw new InvalidOperationException("Commission session cannot be approved without at least one member.");
        if (_minutes.Count == 0 && _decisions.Count == 0)
            throw new InvalidOperationException("Commission session cannot be approved without at least one minute or decision.");
        Status = TenderCommissionSessionStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedByUserId = userId;
    }

    public void Cancel(string reason, Guid userId)
    {
        EnsureNotFinal();
        Status = TenderCommissionSessionStatus.Cancelled;
        CancellationReason = Required(reason, nameof(reason));
        CancelledAt = DateTime.UtcNow;
        CancelledByUserId = userId;
    }

    public void AddMember(TenderCommissionMember member)
    {
        EnsureEditable();
        if (member.SessionId != Id) throw new InvalidOperationException("Commission member belongs to another session.");
        if (_members.Any(x => x.UserId == member.UserId))
            throw new InvalidOperationException("User is already a member of this commission session.");
        EnsureUniqueRole(member.Role, member.Id);
        _members.Add(member);
    }

    public void UpdateMember(Guid memberId, TenderCommissionMemberRole role,
        TenderCommissionAttendanceStatus attendanceStatus, TenderCommissionVoteStatus? voteStatus, string? voteNote)
    {
        EnsureEditable();
        var member = _members.SingleOrDefault(x => x.Id == memberId)
            ?? throw new InvalidOperationException("Commission member was not found.");
        EnsureUniqueRole(role, member.Id);
        member.Update(role, attendanceStatus, voteStatus, voteNote);
    }

    public void RemoveMember(Guid memberId)
    {
        EnsureEditable();
        var member = _members.SingleOrDefault(x => x.Id == memberId)
            ?? throw new InvalidOperationException("Commission member was not found.");
        _members.Remove(member);
    }

    public void AddAgendaItem(TenderCommissionAgendaItem item)
    {
        EnsureEditable();
        if (item.SessionId != Id) throw new InvalidOperationException("Agenda item belongs to another session.");
        _agendaItems.Add(item);
    }

    public void UpdateAgendaItem(Guid id, string title, string? description, int orderNo,
        TenderCommissionAgendaStatus status, string? notes)
    {
        EnsureEditable();
        var item = _agendaItems.SingleOrDefault(x => x.Id == id)
            ?? throw new InvalidOperationException("Agenda item was not found.");
        item.Update(title, description, orderNo, status, notes);
    }

    public void AddMinute(TenderCommissionMinute minute)
    {
        EnsureEditable();
        if (minute.SessionId != Id) throw new InvalidOperationException("Minute belongs to another session.");
        _minutes.Add(minute);
    }

    public void UpdateMinute(Guid id, string text, Guid updatedByUserId)
    {
        EnsureEditable();
        var minute = _minutes.SingleOrDefault(x => x.Id == id)
            ?? throw new InvalidOperationException("Minute was not found.");
        minute.Update(text, updatedByUserId);
    }

    public void AddDecision(TenderCommissionDecision decision)
    {
        EnsureEditable();
        if (decision.SessionId != Id) throw new InvalidOperationException("Decision belongs to another session.");
        _decisions.Add(decision);
    }

    public TenderCommissionDecision ApproveDecision(Guid decisionId, Guid approvedByUserId)
    {
        EnsureNotFinal();
        var decision = _decisions.SingleOrDefault(x => x.Id == decisionId)
            ?? throw new InvalidOperationException("Decision was not found.");
        decision.Approve(approvedByUserId);
        return decision;
    }

    public void RejectDecision(Guid decisionId, Guid approvedByUserId)
    {
        EnsureNotFinal();
        var decision = _decisions.SingleOrDefault(x => x.Id == decisionId)
            ?? throw new InvalidOperationException("Decision was not found.");
        decision.Reject(approvedByUserId);
    }

    public void AddAttachment(TenderCommissionAttachment attachment)
    {
        EnsureNotFinal();
        if (attachment.SessionId != Id) throw new InvalidOperationException("Attachment belongs to another session.");
        _attachments.Add(attachment);
    }

    private void EnsureUniqueRole(TenderCommissionMemberRole role, Guid currentMemberId)
    {
        if (role == TenderCommissionMemberRole.Chairperson && _members.Any(x => x.Role == role && x.Id != currentMemberId))
            throw new InvalidOperationException("Only one chairperson is allowed per commission session.");
        if (role == TenderCommissionMemberRole.Secretary && _members.Any(x => x.Role == role && x.Id != currentMemberId))
            throw new InvalidOperationException("Only one secretary is allowed per commission session.");
    }

    private void EnsureEditable()
    {
        EnsureNotFinal();
        if (Status is TenderCommissionSessionStatus.Completed or TenderCommissionSessionStatus.Approved)
            throw new InvalidOperationException("Completed or approved commission sessions are read-only.");
    }

    private void EnsureNotFinal()
    {
        if (Status is TenderCommissionSessionStatus.Approved or TenderCommissionSessionStatus.Cancelled)
            throw new InvalidOperationException("Approved or cancelled commission sessions are read-only.");
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class TenderCommissionMember : AuditableEntity<Guid>
{
    private TenderCommissionMember() : base(Guid.Empty) { FullNameSnapshot = string.Empty; }
    public TenderCommissionMember(Guid id, Guid sessionId, Guid userId, string fullNameSnapshot,
        string? positionSnapshot, Guid? departmentId, TenderCommissionMemberRole role) : base(id)
    {
        SessionId = sessionId;
        UserId = userId;
        FullNameSnapshot = Required(fullNameSnapshot, nameof(fullNameSnapshot));
        PositionSnapshot = Trim(positionSnapshot);
        DepartmentId = departmentId;
        Role = role;
        AttendanceStatus = TenderCommissionAttendanceStatus.Invited;
        VoteStatus = TenderCommissionVoteStatus.NotVoted;
    }
    public Guid SessionId { get; private set; }
    public Guid UserId { get; private set; }
    public string FullNameSnapshot { get; private set; }
    public string? PositionSnapshot { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public TenderCommissionMemberRole Role { get; private set; }
    public TenderCommissionAttendanceStatus AttendanceStatus { get; private set; }
    public TenderCommissionVoteStatus? VoteStatus { get; private set; }
    public string? VoteNote { get; private set; }
    public DateTime? SignedAt { get; private set; }
    public void Update(TenderCommissionMemberRole role, TenderCommissionAttendanceStatus attendanceStatus,
        TenderCommissionVoteStatus? voteStatus, string? voteNote)
    {
        Role = role;
        AttendanceStatus = attendanceStatus;
        VoteStatus = voteStatus;
        VoteNote = Trim(voteNote);
        if (voteStatus is not null and not TenderCommissionVoteStatus.NotVoted)
            SignedAt ??= DateTime.UtcNow;
    }
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class TenderCommissionAgendaItem : AuditableEntity<Guid>
{
    private TenderCommissionAgendaItem() : base(Guid.Empty) { Title = string.Empty; }
    public TenderCommissionAgendaItem(Guid id, Guid sessionId, int orderNo, string title, string? description,
        Guid? relatedTenderBidId, Guid? relatedSupplierId) : base(id)
    {
        SessionId = sessionId;
        OrderNo = orderNo;
        Title = Required(title, nameof(title));
        Description = Trim(description);
        RelatedTenderBidId = relatedTenderBidId;
        RelatedSupplierId = relatedSupplierId;
        Status = TenderCommissionAgendaStatus.Pending;
    }
    public Guid SessionId { get; private set; }
    public int OrderNo { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public Guid? RelatedTenderBidId { get; private set; }
    public Guid? RelatedSupplierId { get; private set; }
    public TenderCommissionAgendaStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public void Update(string title, string? description, int orderNo, TenderCommissionAgendaStatus status, string? notes)
    {
        Title = Required(title, nameof(title));
        Description = Trim(description);
        OrderNo = orderNo;
        Status = status;
        Notes = Trim(notes);
    }
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class TenderCommissionMinute : AuditableEntity<Guid>
{
    private TenderCommissionMinute() : base(Guid.Empty) { Text = string.Empty; }
    public TenderCommissionMinute(Guid id, Guid sessionId, Guid? agendaItemId, string text, Guid createdByUserId) : base(id)
    {
        SessionId = sessionId;
        AgendaItemId = agendaItemId;
        Text = Required(text, nameof(text));
        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
    }
    public Guid SessionId { get; private set; }
    public Guid? AgendaItemId { get; private set; }
    public string Text { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }
    public void Update(string text, Guid updatedByUserId)
    {
        Text = Required(text, nameof(text));
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}

public sealed class TenderCommissionDecision : AuditableEntity<Guid>
{
    private TenderCommissionDecision() : base(Guid.Empty) { DecisionText = string.Empty; }
    public TenderCommissionDecision(Guid id, Guid sessionId, TenderCommissionDecisionType decisionType, Guid tenderId,
        Guid? selectedTenderBidId, Guid? selectedSupplierId, string decisionText, string? reason, Guid createdByUserId) : base(id)
    {
        if (decisionType is TenderCommissionDecisionType.RecommendWinner or TenderCommissionDecisionType.ApproveWinner
            && (!selectedTenderBidId.HasValue || !selectedSupplierId.HasValue))
            throw new InvalidOperationException("Winner decisions require selected bid and supplier.");
        SessionId = sessionId;
        DecisionType = decisionType;
        TenderId = tenderId;
        SelectedTenderBidId = selectedTenderBidId;
        SelectedSupplierId = selectedSupplierId;
        DecisionText = Required(decisionText, nameof(decisionText));
        Reason = Trim(reason);
        Status = TenderCommissionDecisionStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
    }
    public Guid SessionId { get; private set; }
    public TenderCommissionDecisionType DecisionType { get; private set; }
    public Guid TenderId { get; private set; }
    public Guid? SelectedTenderBidId { get; private set; }
    public Guid? SelectedSupplierId { get; private set; }
    public string DecisionText { get; private set; }
    public string? Reason { get; private set; }
    public TenderCommissionDecisionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public void Approve(Guid approvedByUserId)
    {
        Status = TenderCommissionDecisionStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedByUserId = approvedByUserId;
    }
    public void Reject(Guid rejectedByUserId)
    {
        Status = TenderCommissionDecisionStatus.Rejected;
        ApprovedAt = DateTime.UtcNow;
        ApprovedByUserId = rejectedByUserId;
    }
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class TenderCommissionAttachment : AuditableEntity<Guid>
{
    private TenderCommissionAttachment() : base(Guid.Empty) { DocumentType = string.Empty; }
    public TenderCommissionAttachment(Guid id, Guid sessionId, Guid? fileDocumentId, string documentType,
        string? originalFileName, string? description, Guid uploadedByUserId) : base(id)
    {
        SessionId = sessionId;
        FileDocumentId = fileDocumentId;
        DocumentType = string.IsNullOrWhiteSpace(documentType) ? "CommissionDocument" : documentType.Trim();
        OriginalFileName = Trim(originalFileName);
        Description = Trim(description);
        UploadedAt = DateTime.UtcNow;
        UploadedByUserId = uploadedByUserId;
    }
    public Guid SessionId { get; private set; }
    public Guid? FileDocumentId { get; private set; }
    public string DocumentType { get; private set; }
    public string? OriginalFileName { get; private set; }
    public string? Description { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class TenderCommissionSessionSequence : AuditableEntity<Guid>
{
    private TenderCommissionSessionSequence() : base(Guid.Empty) { }
    public TenderCommissionSessionSequence(Guid id, int year, int lastNumber) : base(id)
    {
        Year = year;
        LastNumber = lastNumber;
    }
    public int Year { get; private set; }
    public int LastNumber { get; private set; }
    public int Next() => ++LastNumber;
}
