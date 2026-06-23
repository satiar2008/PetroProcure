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
        var item = new MescItem(Guid.NewGuid(), "6543210001", "Test item", "EA");

        Assert.Equal("654321", item.GeneralGroupCode);
    }

    [Fact]
    public void Constructor_ThrowsWhenCodeIsShorterThanSixDigits()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new MescItem(Guid.NewGuid(), "12345", "Invalid item", "EA"));

        Assert.Contains("at least 6 digits", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LinkGeneralGroup_LinksMatchingGroup()
    {
        var item = new MescItem(Guid.NewGuid(), "1234567890", "Test item", "EA");
        var group = new MescGeneralGroup(Guid.NewGuid(), "123456", "General description");

        item.LinkGeneralGroup(group);

        Assert.Same(group, item.GeneralGroup);
    }
}
