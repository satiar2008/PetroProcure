using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.PurchaseFiles;

public sealed class PurchaseFileNote : Entity<Guid>
{
    public PurchaseFileNote(
        Guid id, Guid purchaseFileId, Guid departmentId, Guid userId,
        string noteText, bool isInternal)
        : base(id)
    {
        PurchaseFileId = purchaseFileId;
        DepartmentId = departmentId;
        UserId = userId;
        NoteText = string.IsNullOrWhiteSpace(noteText)
            ? throw new ArgumentException("Note text is required.", nameof(noteText))
            : noteText.Trim();
        CreatedAt = DateTime.UtcNow;
        IsInternal = isInternal;
    }

    public Guid PurchaseFileId { get; private set; }
    public Guid DepartmentId { get; private set; }
    public Guid UserId { get; private set; }
    public string NoteText { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsInternal { get; private set; }
}
