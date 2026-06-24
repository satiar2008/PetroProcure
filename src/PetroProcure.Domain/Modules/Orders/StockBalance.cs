using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Orders;

public sealed class StockBalance : AuditableEntity<Guid>
{
    private StockBalance() : base(Guid.Empty) { }

    public StockBalance(Guid id, Guid mescItemId, Guid? warehouseId, decimal availableQuantity,
        decimal reservedQuantity = 0, decimal onOrderQuantity = 0) : base(id)
    {
        MescItemId = mescItemId;
        WarehouseId = warehouseId;
        SetQuantities(availableQuantity, reservedQuantity, onOrderQuantity);
    }

    public Guid MescItemId { get; private set; }
    public Guid? WarehouseId { get; private set; }
    public decimal AvailableQuantity { get; private set; }
    public decimal ReservedQuantity { get; private set; }
    public decimal OnOrderQuantity { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }

    public void SetQuantities(decimal availableQuantity, decimal reservedQuantity, decimal onOrderQuantity)
    {
        if (availableQuantity < 0 || reservedQuantity < 0 || onOrderQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(availableQuantity), "Stock quantities cannot be negative.");
        AvailableQuantity = availableQuantity;
        ReservedQuantity = reservedQuantity;
        OnOrderQuantity = onOrderQuantity;
        LastUpdatedAt = DateTime.UtcNow;
    }
}
