using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Orders;

public sealed class InventoryControlItem : AuditableEntity<Guid>
{
    private InventoryControlItem() : base(Guid.Empty) { }

    public InventoryControlItem(Guid id, Guid mescItemId, string mescCode, string mescGeneralGroupCode,
        string generalDescription, string specificDescription, Guid unitOfMeasureId, decimal minimumStockLevel,
        decimal reorderPoint, decimal? maximumStockLevel, decimal? safetyStock, bool isStockControlled,
        string? notes = null) : base(id)
    {
        if (minimumStockLevel < 0) throw new ArgumentOutOfRangeException(nameof(minimumStockLevel));
        if (reorderPoint < 0) throw new ArgumentOutOfRangeException(nameof(reorderPoint));
        MescItemId = mescItemId;
        MescCode = Required(mescCode, nameof(mescCode));
        MescGeneralGroupCode = Required(mescGeneralGroupCode, nameof(mescGeneralGroupCode));
        GeneralDescription = Required(generalDescription, nameof(generalDescription));
        SpecificDescription = Required(specificDescription, nameof(specificDescription));
        UnitOfMeasureId = unitOfMeasureId;
        MinimumStockLevel = minimumStockLevel;
        ReorderPoint = reorderPoint;
        MaximumStockLevel = maximumStockLevel;
        SafetyStock = safetyStock;
        IsStockControlled = isStockControlled;
        IsActive = true;
        Notes = notes?.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public Guid MescItemId { get; private set; }
    public string MescCode { get; private set; } = string.Empty;
    public string MescGeneralGroupCode { get; private set; } = string.Empty;
    public string GeneralDescription { get; private set; } = string.Empty;
    public string SpecificDescription { get; private set; } = string.Empty;
    public Guid UnitOfMeasureId { get; private set; }
    public decimal MinimumStockLevel { get; private set; }
    public decimal ReorderPoint { get; private set; }
    public decimal? MaximumStockLevel { get; private set; }
    public decimal? SafetyStock { get; private set; }
    public bool IsStockControlled { get; private set; }
    public bool IsActive { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public void Update(decimal minimumStockLevel, decimal reorderPoint, decimal? maximumStockLevel,
        decimal? safetyStock, bool isStockControlled, bool isActive, string? notes)
    {
        if (minimumStockLevel < 0) throw new ArgumentOutOfRangeException(nameof(minimumStockLevel));
        if (reorderPoint < 0) throw new ArgumentOutOfRangeException(nameof(reorderPoint));
        MinimumStockLevel = minimumStockLevel;
        ReorderPoint = reorderPoint;
        MaximumStockLevel = maximumStockLevel;
        SafetyStock = safetyStock;
        IsStockControlled = isStockControlled;
        IsActive = isActive;
        Notes = notes?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}
