using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.PurchaseOrders;
using PetroProcure.Domain.Modules.Warehouse;

namespace PetroProcure.UnitTests.Domain;

public sealed class WarehouseReceiptTests
{
    [Fact]
    public void SubmitRejectsReceiptWithoutItems()
    {
        var receipt = Receipt();

        var ex = Assert.Throws<InvalidOperationException>(() => receipt.Submit());

        Assert.Contains("without items", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApproveChangesReceiptStatusAndStoresApprover()
    {
        var approverId = Guid.NewGuid();
        var receipt = Receipt();
        receipt.AddItem(Item(receipt.Id, receivedQuantity: 4, remainingAfterReceipt: 6));

        receipt.Submit();
        receipt.Approve(approverId);

        Assert.Equal(WarehouseReceiptStatus.Approved, receipt.Status);
        Assert.Equal(approverId, receipt.ApprovedByUserId);
        Assert.NotNull(receipt.ApprovedAt);
    }

    [Fact]
    public void ApprovedReceiptIsReadOnly()
    {
        var receipt = Receipt();
        receipt.AddItem(Item(receipt.Id, receivedQuantity: 2, remainingAfterReceipt: 8));
        receipt.Submit();
        receipt.Approve(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => receipt.Update(DateTime.UtcNow, null, null, null, null));
    }

    [Fact]
    public void PurchaseOrderItemReceiveUpdatesReceivedAndRemainingQuantities()
    {
        var item = PurchaseOrderItem(Guid.NewGuid(), orderedQuantity: 10);

        item.Receive(3);

        Assert.Equal(3, item.ReceivedQuantity);
        Assert.Equal(7, item.RemainingQuantity);
    }

    [Fact]
    public void PurchaseOrderItemReceiveRejectsOverReceipt()
    {
        var item = PurchaseOrderItem(Guid.NewGuid(), orderedQuantity: 10);

        Assert.Throws<InvalidOperationException>(() => item.Receive(11));
    }

    [Fact]
    public void WarehouseReceiptSequenceIncrementsPerCall()
    {
        var sequence = new WarehouseReceiptSequence(Guid.NewGuid(), 2026, 1);

        Assert.Equal(2, sequence.Next());
        Assert.Equal(3, sequence.Next());
    }

    private static WarehouseReceipt Receipt() => new(Guid.NewGuid(), "WR-2026-000001",
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid());

    private static WarehouseReceiptItem Item(Guid receiptId, decimal receivedQuantity, decimal remainingAfterReceipt) =>
        new(Guid.NewGuid(), receiptId, Guid.NewGuid(), Guid.NewGuid(), "1234560001", "123456",
            "لوله و اتصالات", "لوله فولادی", Guid.NewGuid(), 10, 0, receivedQuantity,
            remainingAfterReceipt, WarehouseReceiptQualityStatus.Accepted);

    private static PurchaseOrderItem PurchaseOrderItem(Guid purchaseOrderId, decimal orderedQuantity) =>
        new(Guid.NewGuid(), purchaseOrderId, null, null, null, Guid.NewGuid(), "1234560001",
            "123456", "لوله و اتصالات", "لوله فولادی", Guid.NewGuid(), orderedQuantity);
}
