using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Tenders;

namespace PetroProcure.UnitTests.Domain;

public sealed class TenderDomainTests
{
    [Fact]
    public void Tender_cannot_be_published_without_items()
    {
        var tender = CreateTender();
        tender.AddParticipant(CreateParticipant(tender.Id));

        var ex = Assert.Throws<InvalidOperationException>(() => tender.Publish(Guid.NewGuid()));

        Assert.Contains("without at least one item", ex.Message);
    }

    [Fact]
    public void Tender_cannot_be_published_without_participants()
    {
        var tender = CreateTender();
        tender.AddItem(CreateItem(tender.Id));

        var ex = Assert.Throws<InvalidOperationException>(() => tender.Publish(Guid.NewGuid()));

        Assert.Contains("without at least one participant", ex.Message);
    }

    [Fact]
    public void Tender_item_keeps_purchase_file_snapshot()
    {
        var tender = CreateTender();
        var item = CreateItem(tender.Id);

        tender.AddItem(item);

        Assert.Equal("1234567890", tender.Items.Single().MescCode);
        Assert.Equal("Pumps", tender.Items.Single().GeneralDescription);
    }

    [Fact]
    public void Supplier_snapshot_is_kept_when_participant_added()
    {
        var tender = CreateTender();
        tender.AddParticipant(CreateParticipant(tender.Id));

        var participant = tender.Participants.Single();
        Assert.Equal("SUP-001", participant.SupplierCode);
        Assert.Equal("Acme Supplier", participant.SupplierName);
    }

    [Fact]
    public void Only_one_winner_can_be_selected()
    {
        var tender = ReadyTenderWithBid();
        var bid = tender.Bids.Single();

        tender.SelectWinner(bid.Id, Guid.NewGuid(), "best offer", null);

        Assert.Throws<InvalidOperationException>(() => tender.SelectWinner(bid.Id, Guid.NewGuid(), "again", null));
    }

    [Fact]
    public void Closed_tender_is_read_only()
    {
        var tender = ReadyTenderWithBid();
        tender.Close(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => tender.AddItem(CreateItem(tender.Id)));
    }

    [Fact]
    public void Cancelled_tender_is_read_only()
    {
        var tender = CreateTender();
        tender.Cancel("cancelled", Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => tender.AddParticipant(CreateParticipant(tender.Id)));
    }

    private static Tender ReadyTenderWithBid()
    {
        var tender = CreateTender();
        var participant = CreateParticipant(tender.Id);
        tender.AddItem(CreateItem(tender.Id));
        tender.AddParticipant(participant);
        tender.Publish(Guid.NewGuid());
        tender.AddBid(new TenderBid(Guid.NewGuid(), tender.Id, participant.Id, participant.SupplierId, "B-1", "IRR", 100, 100, null, null, null, null, Guid.NewGuid()));
        return tender;
    }

    private static Tender CreateTender() =>
        new(Guid.NewGuid(), "TND-2026-000001", Guid.NewGuid(), null, "Tender", TenderType.LimitedTender,
            DateTime.UtcNow, null, null, null, Guid.NewGuid());

    private static TenderItem CreateItem(Guid tenderId) =>
        new(Guid.NewGuid(), tenderId, Guid.NewGuid(), Guid.NewGuid(), "1234567890", "123456",
            "Pumps", "Centrifugal pump", Guid.NewGuid(), 2, "API pump");

    private static TenderParticipant CreateParticipant(Guid tenderId) =>
        new(Guid.NewGuid(), tenderId, Guid.NewGuid(), "SUP-001", "Acme Supplier", null, null, null);
}
