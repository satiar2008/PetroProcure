using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.PurchaseOrders;

namespace PetroProcure.UnitTests.Domain;

public class PurchaseOrderTests
{
    [Fact]
    public void Cannot_submit_without_items()
    {
        var po = NewPurchaseOrder();
        Assert.Throws<InvalidOperationException>(() => po.Submit(Guid.NewGuid()));
    }

    [Fact]
    public void Cannot_issue_unless_approved()
    {
        var po = NewPurchaseOrder();
        po.AddItem(NewItem(po.Id));
        po.Submit(Guid.NewGuid());
        Assert.Throws<InvalidOperationException>(() => po.Issue(Guid.NewGuid()));
    }

    [Fact]
    public void Approved_purchase_order_can_be_issued_and_becomes_readonly()
    {
        var po = NewPurchaseOrder();
        po.AddItem(NewItem(po.Id));
        po.Submit(Guid.NewGuid());
        po.Approve(Guid.NewGuid());
        po.Issue(Guid.NewGuid());
        Assert.Equal(PurchaseOrderStatus.Issued, po.Status);
        Assert.Throws<InvalidOperationException>(() => po.AddItem(NewItem(po.Id)));
    }

    [Fact]
    public void Remaining_quantity_equals_ordered_quantity_initially()
    {
        var item = NewItem(Guid.NewGuid(), orderedQuantity: 12);
        Assert.Equal(12, item.OrderedQuantity);
        Assert.Equal(0, item.ReceivedQuantity);
        Assert.Equal(12, item.RemainingQuantity);
    }

    [Fact]
    public void Item_snapshot_values_are_stored()
    {
        var item = NewItem(Guid.NewGuid(), mescCode: "1234560001", generalDescription: "لوله و اتصالات");
        Assert.Equal("1234560001", item.MescCode);
        Assert.Equal("123456", item.MescGeneralGroupCode);
        Assert.Equal("لوله و اتصالات", item.GeneralDescription);
        Assert.Equal("لوله فولادی", item.SpecificDescription);
    }

    private static PurchaseOrder NewPurchaseOrder() =>
        new(Guid.NewGuid(), "PO-2026-000001", Guid.NewGuid(), Guid.NewGuid(), null, null, null,
            "سفارش خرید تست", PurchaseOrderType.DirectPurchase, "IRR", Guid.NewGuid());

    private static PurchaseOrderItem NewItem(Guid purchaseOrderId, decimal orderedQuantity = 5,
        string mescCode = "1234560001", string generalDescription = "گروه عمومی") =>
        new(Guid.NewGuid(), purchaseOrderId, Guid.NewGuid(), null, null, Guid.NewGuid(), mescCode,
            "123456", generalDescription, "لوله فولادی", Guid.NewGuid(), orderedQuantity, 1000);
}
