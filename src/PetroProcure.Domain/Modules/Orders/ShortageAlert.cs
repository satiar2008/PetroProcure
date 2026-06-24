using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Orders;

public sealed class ShortageAlert : AuditableEntity<Guid>
{
    private ShortageAlert() : base(Guid.Empty) { }

    public ShortageAlert(Guid id, Guid mescItemId, string mescCode, string mescGeneralGroupCode,
        string generalDescription, string specificDescription, Guid unitOfMeasureId, decimal currentStock,
        decimal reorderPoint) : base(id)
    {
        if (reorderPoint < 0) throw new ArgumentOutOfRangeException(nameof(reorderPoint));
        MescItemId = mescItemId;
        MescCode = Required(mescCode, nameof(mescCode));
        MescGeneralGroupCode = Required(mescGeneralGroupCode, nameof(mescGeneralGroupCode));
        GeneralDescription = Required(generalDescription, nameof(generalDescription));
        SpecificDescription = Required(specificDescription, nameof(specificDescription));
        UnitOfMeasureId = unitOfMeasureId;
        CurrentStock = currentStock;
        ReorderPoint = reorderPoint;
        ShortageQuantity = CalculateShortage(currentStock, reorderPoint);
        Status = ShortageAlertStatus.Open;
        DetectedAt = DateTime.UtcNow;
    }

    public Guid MescItemId { get; private set; }
    public string MescCode { get; private set; } = string.Empty;
    public string MescGeneralGroupCode { get; private set; } = string.Empty;
    public string GeneralDescription { get; private set; } = string.Empty;
    public string SpecificDescription { get; private set; } = string.Empty;
    public Guid UnitOfMeasureId { get; private set; }
    public decimal CurrentStock { get; private set; }
    public decimal ReorderPoint { get; private set; }
    public decimal ShortageQuantity { get; private set; }
    public ShortageAlertStatus Status { get; private set; }
    public DateTime DetectedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public Guid? RelatedIndentId { get; private set; }
    public string? ResolutionNote { get; private set; }

    public static decimal CalculateShortage(decimal currentStock, decimal reorderPoint) =>
        currentStock < reorderPoint ? reorderPoint - currentStock : 0;

    public void MarkConvertedToIndent(Guid indentId)
    {
        if (Status is ShortageAlertStatus.Resolved or ShortageAlertStatus.Cancelled)
            throw new InvalidOperationException("Resolved or cancelled shortage alerts cannot be converted.");
        RelatedIndentId = indentId;
        Status = ShortageAlertStatus.ConvertedToIndent;
        ResolvedAt = DateTime.UtcNow;
    }

    public void Resolve(string? note)
    {
        if (Status == ShortageAlertStatus.ConvertedToIndent) throw new InvalidOperationException("Converted alerts are already closed.");
        Status = ShortageAlertStatus.Resolved;
        ResolutionNote = note?.Trim();
        ResolvedAt = DateTime.UtcNow;
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}
