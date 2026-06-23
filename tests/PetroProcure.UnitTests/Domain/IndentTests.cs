using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Indents;

namespace PetroProcure.UnitTests.Domain;

public sealed class IndentTests
{
    [Fact]
    public void BuildIndentNumber_ReturnsSevenDigitNumber()
    {
        var indentNumber = Indent.BuildIndentNumber(26, 7, 120);

        Assert.Equal("2670120", indentNumber);
    }

    [Theory]
    [InlineData(0, IndentType.DirectPurchase)]
    [InlineData(1, IndentType.DirectPurchase)]
    [InlineData(2, IndentType.DirectPurchase)]
    [InlineData(3, IndentType.Manual)]
    [InlineData(4, IndentType.Manual)]
    [InlineData(5, IndentType.SystemGenerated)]
    [InlineData(6, IndentType.SystemGenerated)]
    [InlineData(7, IndentType.SystemGenerated)]
    [InlineData(8, IndentType.SystemGenerated)]
    [InlineData(9, IndentType.SystemGenerated)]
    public void ResolveIndentType_ReturnsExpectedType(int typeDigit, IndentType expectedType)
    {
        var indentType = Indent.ResolveIndentType(typeDigit);

        Assert.Equal(expectedType, indentType);
    }

    [Fact]
    public void Constructor_StoresIndentNumberParts()
    {
        var indent = new Indent(
            Guid.NewGuid(), "2630145", 26, 3, 145, "Test indent",
            Guid.NewGuid(), null, Guid.NewGuid());

        Assert.Equal("2630145", indent.IndentNumber);
        Assert.Equal(26, indent.YearPart);
        Assert.Equal(3, indent.TypeDigit);
        Assert.Equal(145, indent.Sequence);
        Assert.Equal(IndentType.Manual, indent.IndentType);
    }
}
