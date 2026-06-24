using PetroProcure.Domain.Modules.Items;

namespace PetroProcure.UnitTests.Domain;

public sealed class MescItemTests
{
    [Fact]
    public void GetGeneralGroupCode_ReturnsFirstSixDigits()
    {
        var groupCode = MescItem.GetGeneralGroupCode("1234567890");

        Assert.Equal("123456", groupCode);
    }

    [Fact]
    public void Constructor_DerivesGeneralGroupCodeFromCode()
    {
        var unitId = Guid.NewGuid();
        var item = new MescItem(Guid.NewGuid(), "6543210001", "Test item", "EA", unitId);

        Assert.Equal("654321", item.GeneralGroupCode);
        Assert.Equal(unitId, item.UnitOfMeasureId);
    }

    [Fact]
    public void Constructor_ThrowsWhenCodeIsShorterThanSixDigits()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new MescItem(Guid.NewGuid(), "12345", "Invalid item", "EA", Guid.NewGuid()));

        Assert.Contains("at least 6 digits", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LinkGeneralGroup_LinksMatchingGroup()
    {
        var item = new MescItem(Guid.NewGuid(), "1234567890", "Test item", "EA", Guid.NewGuid());
        var group = new MescGeneralGroup(Guid.NewGuid(), "123456", "General description");

        item.LinkGeneralGroup(group);

        Assert.Same(group, item.GeneralGroup);
    }
}
