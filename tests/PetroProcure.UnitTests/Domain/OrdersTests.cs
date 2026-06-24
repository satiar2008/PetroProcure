using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Orders;

namespace PetroProcure.UnitTests.Domain;

public sealed class OrdersTests
{
    [Fact]
    public void Material_need_requires_mesc_item()
    {
        Assert.Throws<ArgumentException>(() => new MaterialNeed(Guid.NewGuid(), "MN-2026-000001",
            Guid.Empty, "1234567890", "123456", "General", "Specific", Guid.NewGuid(), 1,
            null, Guid.NewGuid(), null, Guid.NewGuid(), MaterialNeedPriority.Normal));
    }

    [Fact]
    public void Approved_material_need_can_be_converted_to_indent()
    {
        var need = Need();
        need.Submit();
        need.Approve(Guid.NewGuid());
        var indentId = Guid.NewGuid();
        need.MarkConvertedToIndent(indentId);
        Assert.Equal(MaterialNeedStatus.ConvertedToIndent, need.Status);
        Assert.Equal(indentId, need.RelatedIndentId);
    }

    [Fact]
    public void Rejected_material_need_cannot_be_converted()
    {
        var need = Need();
        need.Submit();
        need.Reject(Guid.NewGuid(), "Not required");
        Assert.Throws<InvalidOperationException>(() => need.MarkConvertedToIndent(Guid.NewGuid()));
    }

    [Fact]
    public void Shortage_alert_calculation_returns_missing_quantity()
    {
        Assert.Equal(7, ShortageAlert.CalculateShortage(3, 10));
        Assert.Equal(0, ShortageAlert.CalculateShortage(10, 10));
    }

    [Fact]
    public void Shortage_alert_can_be_converted_to_indent()
    {
        var alert = new ShortageAlert(Guid.NewGuid(), Guid.NewGuid(), "1234567890", "123456",
            "General", "Specific", Guid.NewGuid(), 2, 5);
        var indentId = Guid.NewGuid();
        alert.MarkConvertedToIndent(indentId);
        Assert.Equal(ShortageAlertStatus.ConvertedToIndent, alert.Status);
        Assert.Equal(indentId, alert.RelatedIndentId);
    }

    [Fact]
    public void Mesc_snapshots_are_copied_to_material_need()
    {
        var need = Need();
        Assert.Equal("1234567890", need.MescCode);
        Assert.Equal("123456", need.MescGeneralGroupCode);
        Assert.Equal("General", need.GeneralDescription);
        Assert.Equal("Specific", need.SpecificDescription);
    }

    private static MaterialNeed Need() => new(Guid.NewGuid(), "MN-2026-000001",
        Guid.NewGuid(), "1234567890", "123456", "General", "Specific", Guid.NewGuid(), 10,
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), Guid.NewGuid(), null, Guid.NewGuid(),
        MaterialNeedPriority.High, "Need");
}
