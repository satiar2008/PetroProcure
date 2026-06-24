using System.Globalization;

namespace PetroProcure.Web.Services;

public interface IPersianDateService
{
    string FormatDate(DateTime? value);
    string FormatDateTime(DateTime? value);
    string FormatDate(DateOnly? value);
}

public sealed class PersianDateService : IPersianDateService
{
    private static readonly PersianCalendar Calendar = new();

    public string FormatDate(DateTime? value)
    {
        if (!value.HasValue) return "—";
        var local = value.Value.Kind == DateTimeKind.Utc ? value.Value.ToLocalTime() : value.Value;
        return Format(local, includeTime: false);
    }

    public string FormatDateTime(DateTime? value)
    {
        if (!value.HasValue) return "—";
        var local = value.Value.Kind == DateTimeKind.Utc ? value.Value.ToLocalTime() : value.Value;
        return Format(local, includeTime: true);
    }

    public string FormatDate(DateOnly? value) =>
        value.HasValue ? Format(value.Value.ToDateTime(TimeOnly.MinValue), includeTime: false) : "—";

    private static string Format(DateTime value, bool includeTime)
    {
        var text = $"{Calendar.GetYear(value):0000}/{Calendar.GetMonth(value):00}/{Calendar.GetDayOfMonth(value):00}";
        return includeTime ? $"{text} {value:HH:mm}" : text;
    }
}
