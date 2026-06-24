using PetroProcure.Web.Services;

namespace PetroProcure.UnitTests.Web;

public sealed class PersianDateUiTests
{
    private readonly PersianDateService _service = new();

    [Fact]
    public void Gregorian_date_is_rendered_as_persian_calendar_date()
    {
        var result = _service.FormatDate(new DateTime(2026, 6, 24));

        Assert.Equal("1405/04/03", result);
    }

    [Fact]
    public void Date_time_rendering_keeps_local_time()
    {
        var result = _service.FormatDateTime(new DateTime(2026, 6, 24, 15, 30, 0, DateTimeKind.Local));

        Assert.Equal("1405/04/03 15:30", result);
    }

    [Fact]
    public void Null_dates_are_rendered_as_dash()
    {
        Assert.Equal("—", _service.FormatDate((DateTime?)null));
        Assert.Equal("—", _service.FormatDate((DateOnly?)null));
    }
}
