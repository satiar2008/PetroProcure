using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.PurchaseFiles;

public sealed class PurchaseFile : AuditableEntity<Guid>
{
    private readonly List<PurchaseFileItem> _items = [];
    private readonly List<PurchaseFileStatusHistory> _statusHistory = [];
    private readonly List<PurchaseFileNote> _notes = [];

    public PurchaseFile(
        Guid id, string fileNumber, string title, string? description,
        PurchaseFilePriority priority, Guid? sourceIndentId, Guid purchaseDepartmentId,
        Guid currentDepartmentId, Guid? responsibleUserId, Guid createdByUserId)
        : base(id)
    {
        FileNumber = Required(fileNumber, nameof(fileNumber));
        Title = Required(title, nameof(title));
        Description = description?.Trim();
        Status = PurchaseFileStatus.Draft;
        Priority = priority;
        SourceIndentId = sourceIndentId;
        PurchaseDepartmentId = purchaseDepartmentId;
        CurrentDepartmentId = currentDepartmentId;
        ResponsibleUserId = responsibleUserId;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
    }

    public string FileNumber { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public PurchaseFileStatus Status { get; private set; }
    public PurchaseFilePriority Priority { get; private set; }
    public Guid? SourceIndentId { get; private set; }
    public Guid PurchaseDepartmentId { get; private set; }
    public Guid CurrentDepartmentId { get; private set; }
    public Guid? ResponsibleUserId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? ArchivedAt { get; private set; }
    public IReadOnlyCollection<PurchaseFileItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<PurchaseFileStatusHistory> StatusHistory => _statusHistory.AsReadOnly();
    public IReadOnlyCollection<PurchaseFileNote> Notes => _notes.AsReadOnly();

    public void AddItem(PurchaseFileItem item, bool isAdmin = false)
    {
        EnsureEditable(isAdmin);
        ArgumentNullException.ThrowIfNull(item);
        if (item.PurchaseFileId != Id) throw new InvalidOperationException("Item belongs to another purchase file.");
        _items.Add(item);
    }

    public void RemoveItem(Guid itemId, bool isAdmin = false)
    {
        EnsureEditable(isAdmin);
        var item = _items.SingleOrDefault(candidate => candidate.Id == itemId)
            ?? throw new InvalidOperationException("Purchase file item was not found.");
        _items.Remove(item);
    }

    public void AssignDepartment(Guid departmentId, Guid? responsibleUserId, Guid changedByUserId, string? reason, bool isAdmin = false)
    {
        EnsureEditable(isAdmin);
        CurrentDepartmentId = departmentId;
        ResponsibleUserId = responsibleUserId;
        if (Status != PurchaseFileStatus.InPurchaseDepartment)
            ChangeStatus(PurchaseFileStatus.InPurchaseDepartment, changedByUserId, reason, departmentId, isAdmin);
    }

    public void RouteToDepartment(Guid departmentId)
    {
        EnsureEditable();
        CurrentDepartmentId = departmentId;
    }

    public void ApplyWorkflowTransition(
        Guid departmentId, PurchaseFileStatus status, Guid changedByUserId, string? reason, bool isAdmin = false)
    {
        EnsureEditable(isAdmin);
        CurrentDepartmentId = departmentId;
        if (status == PurchaseFileStatus.Completed)
        {
            Complete(changedByUserId, reason);
            return;
        }
        ChangeStatus(status, changedByUserId, reason, departmentId, isAdmin);
    }

    public void ChangeStatus(
        PurchaseFileStatus status, Guid changedByUserId, string? reason = null,
        Guid? departmentId = null, bool isAdmin = false)
    {
        EnsureEditable(isAdmin);
        if (Status == status) throw new InvalidOperationException("Purchase file is already in the requested status.");
        var previous = Status;
        Status = status;
        _statusHistory.Add(new PurchaseFileStatusHistory(
            Guid.NewGuid(), Id, previous, status, changedByUserId, reason, departmentId));
    }

    public void AddNote(PurchaseFileNote note, bool isAdmin = false)
    {
        EnsureEditable(isAdmin);
        ArgumentNullException.ThrowIfNull(note);
        if (note.PurchaseFileId != Id) throw new InvalidOperationException("Note belongs to another purchase file.");
        _notes.Add(note);
    }

    public void Complete(Guid changedByUserId, string? reason = null)
    {
        EnsureEditable();
        ChangeStatus(PurchaseFileStatus.Completed, changedByUserId, reason, CurrentDepartmentId);
        CompletedAt = DateTime.UtcNow;
    }

    public void Archive(Guid changedByUserId, string? reason = null)
    {
        if (Status != PurchaseFileStatus.Completed)
            throw new InvalidOperationException("Only completed purchase files can be archived.");
        ChangeStatus(PurchaseFileStatus.Archived, changedByUserId, reason, CurrentDepartmentId, true);
        ArchivedAt = DateTime.UtcNow;
    }

    private void EnsureEditable(bool isAdmin = false)
    {
        if (Status == PurchaseFileStatus.Archived)
            throw new InvalidOperationException("Archived purchase files are read-only.");
        if (Status == PurchaseFileStatus.Completed && !isAdmin)
            throw new InvalidOperationException("Completed purchase files can only be edited by an administrator.");
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}
