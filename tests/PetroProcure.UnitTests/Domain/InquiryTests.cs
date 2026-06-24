using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Inquiries;

namespace PetroProcure.UnitTests.Domain;

public sealed class InquiryTests
{
    [Fact]
    public void Inquiry_cannot_be_sent_without_items()
    {
        var inquiry = NewInquiry();
        inquiry.AddSupplier(new InquirySupplier(Guid.NewGuid(), inquiry.Id, Guid.NewGuid(), "SUP-1", "Supplier", null, null, null));

        Assert.Throws<InvalidOperationException>(() => inquiry.Send(Guid.NewGuid()));
    }

    [Fact]
    public void Inquiry_cannot_be_sent_without_suppliers()
    {
        var inquiry = NewInquiry();
        inquiry.AddItem(NewItem(inquiry.Id));

        Assert.Throws<InvalidOperationException>(() => inquiry.Send(Guid.NewGuid()));
    }

    [Fact]
    public void Inquiry_item_keeps_snapshot_values()
    {
        var item = NewItem(Guid.NewGuid());

        Assert.Equal("1234560001", item.MescCode);
        Assert.Equal("لوله", item.GeneralDescription);
    }

    [Fact]
    public void Inquiry_supplier_keeps_snapshot_values()
    {
        var supplier = new InquirySupplier(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "SUP-1", "تأمین‌کننده", null, "علی", "a@example.local");

        Assert.Equal("SUP-1", supplier.SupplierCode);
        Assert.Equal("تأمین‌کننده", supplier.SupplierName);
    }

    [Fact]
    public void Only_one_selected_quote_is_allowed()
    {
        var inquiry = NewInquiry();
        var supplier = new InquirySupplier(Guid.NewGuid(), inquiry.Id, Guid.NewGuid(), "SUP-1", "Supplier", null, null, null);
        inquiry.AddItem(NewItem(inquiry.Id));
        inquiry.AddSupplier(supplier);
        var first = NewQuote(inquiry.Id, supplier);
        var second = NewQuote(inquiry.Id, supplier);
        inquiry.AddQuote(first);
        inquiry.AddQuote(second);

        inquiry.SelectQuote(first.Id, null);
        inquiry.SelectQuote(second.Id, null);

        Assert.False(first.IsSelected);
        Assert.True(second.IsSelected);
    }

    [Fact]
    public void Closed_inquiry_is_read_only()
    {
        var inquiry = NewInquiry();
        inquiry.Close(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => inquiry.AddItem(NewItem(inquiry.Id)));
    }

    private static Inquiry NewInquiry() =>
        new(Guid.NewGuid(), "INQ-2026-000001", Guid.NewGuid(), "استعلام", InquiryType.PriceInquiry, DateTime.UtcNow, null, null, Guid.NewGuid());

    private static InquiryItem NewItem(Guid inquiryId) =>
        new(Guid.NewGuid(), inquiryId, Guid.NewGuid(), Guid.NewGuid(), "1234560001", "123456", "لوله", "لوله فولادی", Guid.NewGuid(), 10, "تست");

    private static SupplierQuote NewQuote(Guid inquiryId, InquirySupplier supplier) =>
        new(Guid.NewGuid(), inquiryId, supplier.SupplierId, supplier.Id, null, null, null, "IRR", null, null, null, 100, null, null, null, null, Guid.NewGuid());
}
