using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Indents;

public sealed class Indent : AuditableEntity<Guid>
{
    private readonly List<IndentItem> _items = [];

    private Indent() : base(Guid.Empty)
    {
        IndentNumber = string.Empty;
        Title = string.Empty;
    }

    public Indent(
        Guid id,
        string indentNumber,
        int yearPart,
        int typeDigit,
        int sequence,
        string title,
        Guid requestingDepartmentId,
        Guid? applicantDepartmentId,
        Guid createdByUserId,
        string? description = null,
        IndentSourceType? sourceType = null,
        Guid? sourceMaterialNeedId = null,
        Guid? sourceShortageAlertId = null,
        string? sourceDescription = null)
        : base(id)
    {
        if (indentNumber != BuildIndentNumber(yearPart, typeDigit, sequence))
            throw new ArgumentException("Indent number does not match its component parts.", nameof(indentNumber));

        IndentNumber = indentNumber;
        YearPart = yearPart;
        TypeDigit = typeDigit;
        Sequence = sequence;
        IndentType = ResolveIndentType(typeDigit);
        Title = Required(title, nameof(title));
        RequestingDepartmentId = requestingDepartmentId;
        ApplicantDepartmentId = applicantDepartmentId;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        Status = IndentStatus.Draft;
        Description = description?.Trim();
        SourceType = sourceType ?? DefaultSourceType(IndentType);
        SourceMaterialNeedId = sourceMaterialNeedId;
        SourceShortageAlertId = sourceShortageAlertId;
        SourceDescription = sourceDescription?.Trim();
        ValidateSource();
    }

    public string IndentNumber { get; private set; }
    public int YearPart { get; private set; }
    public int TypeDigit { get; private set; }
    public int Sequence { get; private set; }
    public IndentType IndentType { get; private set; }
    public string Title { get; private set; }
    public Guid RequestingDepartmentId { get; private set; }
    public Guid? ApplicantDepartmentId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IndentStatus Status { get; private set; }
    public string? Description { get; private set; }
    public IndentSourceType SourceType { get; private set; }
    public Guid? SourceMaterialNeedId { get; private set; }
    public Guid? SourceShortageAlertId { get; private set; }
    public string? SourceDescription { get; private set; }
    public IReadOnlyCollection<IndentItem> Items => _items.AsReadOnly();

    public static string BuildIndentNumber(int yearPart, int typeDigit, int sequence)
    {
        if (yearPart is < 0 or > 99) throw new ArgumentOutOfRangeException(nameof(yearPart));
        ResolveIndentType(typeDigit);
        if (sequence is < 1 or > 9999)
            throw new ArgumentOutOfRangeException(nameof(sequence), "Sequence must be between 1 and 9999.");
        return $"{yearPart:00}{typeDigit}{sequence:0000}";
    }

    public static IndentType ResolveIndentType(int typeDigit) => typeDigit switch
    {
        0 or 1 or 2 => IndentType.DirectPurchase,
        3 or 4 => IndentType.Manual,
        >= 5 and <= 9 => IndentType.SystemGenerated,
        _ => throw new ArgumentOutOfRangeException(nameof(typeDigit), "Type digit must be between 0 and 9.")
    };

    public void AddItem(IndentItem item)
    {
        EnsureDraft();
        ArgumentNullException.ThrowIfNull(item);
        if (item.IndentId != Id) throw new InvalidOperationException("Item belongs to another indent.");
        _items.Add(item);
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureDraft();
        var item = _items.SingleOrDefault(candidate => candidate.Id == itemId)
            ?? throw new InvalidOperationException("Indent item was not found.");
        _items.Remove(item);
    }

    public void Submit()
    {
        EnsureStatus(IndentStatus.Draft);
        if (_items.Count == 0) throw new InvalidOperationException("An indent must contain at least one item before submission.");
        Status = IndentStatus.Submitted;
    }

    public void Approve()
    {
        EnsureStatus(IndentStatus.Submitted);
        Status = IndentStatus.Approved;
    }

    public void Reject()
    {
        EnsureStatus(IndentStatus.Submitted);
        Status = IndentStatus.Rejected;
    }

    public void SendToPurchaseDepartment()
    {
        EnsureStatus(IndentStatus.Approved);
        Status = IndentStatus.SentToPurchaseDepartment;
    }

    public Guid? SourceReferenceId => SourceType switch
    {
        IndentSourceType.MaterialNeed => SourceMaterialNeedId,
        IndentSourceType.ShortageAlert => SourceShortageAlertId,
        _ => null
    };

    public string SourceDisplayText => SourceType switch
    {
        IndentSourceType.Manual => "دستی",
        IndentSourceType.DirectPurchase => "خرید مستقیم",
        IndentSourceType.SystemGenerated => "سیستمی",
        IndentSourceType.MaterialNeed => string.IsNullOrWhiteSpace(SourceDescription) ? "نیاز کالا" : $"نیاز کالا - {SourceDescription}",
        IndentSourceType.ShortageAlert => string.IsNullOrWhiteSpace(SourceDescription) ? "هشدار کمبود" : $"هشدار کمبود - {SourceDescription}",
        IndentSourceType.ApplicantNeed => "نیاز متقاضی",
        _ => "سایر"
    };

    private static IndentSourceType DefaultSourceType(IndentType indentType) => indentType switch
    {
        IndentType.DirectPurchase => IndentSourceType.DirectPurchase,
        IndentType.SystemGenerated => IndentSourceType.SystemGenerated,
        _ => IndentSourceType.Manual
    };

    private void ValidateSource()
    {
        if (SourceType == IndentSourceType.MaterialNeed && SourceMaterialNeedId is null)
            throw new InvalidOperationException("Material need source id is required.");
        if (SourceType == IndentSourceType.ShortageAlert && SourceShortageAlertId is null)
            throw new InvalidOperationException("Shortage alert source id is required.");
    }

    private void EnsureDraft() => EnsureStatus(IndentStatus.Draft);
    private void EnsureStatus(IndentStatus expected)
    {
        if (Status != expected) throw new InvalidOperationException($"Indent must be {expected} for this operation.");
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}
