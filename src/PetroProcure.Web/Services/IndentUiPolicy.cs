using PetroProcure.Contracts.V1.Indents;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Web.Services;

public static class IndentUiPolicy
{
    public const string TypeDigitHelpText = "۰، ۱، ۲ خرید مستقیم؛ ۳، ۴ تقاضای دستی؛ ۵ تا ۹ تقاضای سیستمی";
    public static bool CanView(Func<string, bool> has) => has("Indent.View");
    public static bool CanCreate(Func<string, bool> has) => has("Indent.Create");
    public static bool CanEdit(IndentStatus status, Func<string, bool> has) =>
        status == IndentStatus.Draft && has("Indent.Create");
    public static bool CanSubmit(IndentStatus status, Func<string, bool> has) =>
        status == IndentStatus.Draft && has("Indent.Create");
    public static bool CanApproveOrReject(IndentStatus status, Func<string, bool> has) =>
        status == IndentStatus.Submitted && has("Indent.Approve");
    public static bool CanSendToPurchase(IndentStatus status, Func<string, bool> has) =>
        status == IndentStatus.Approved && has("Indent.SendToPurchase");
    public static bool CanCreatePurchaseFile(IndentStatus status, Func<string, bool> has) =>
        status == IndentStatus.Approved && has("PurchaseFile.Create");

    public static string FormatNumber(string number) =>
        number.Length == 7 ? $"{number[..2]}-{number[2]}-{number[3..]}" : number;

    public static bool IsCreateFormValid(string? title, Guid requestingDepartmentId, int itemCount) =>
        !string.IsNullOrWhiteSpace(title) && requestingDepartmentId != Guid.Empty && itemCount > 0;

    public static string TypeText(IndentType type) => type switch
    {
        IndentType.DirectPurchase => "خرید مستقیم",
        IndentType.Manual => "تقاضای دستی",
        IndentType.SystemGenerated => "تقاضای سیستمی",
        _ => type.ToString()
    };

    public static string StatusText(IndentStatus status) => status switch
    {
        IndentStatus.Draft => "پیش‌نویس",
        IndentStatus.Submitted => "ارسال‌شده برای تأیید",
        IndentStatus.Approved => "تأییدشده",
        IndentStatus.Rejected => "ردشده",
        IndentStatus.SentToPurchaseDepartment => "ارسال‌شده به واحد خرید",
        _ => status.ToString()
    };

    public static IReadOnlyList<IndentGroupedItemsDto> GroupItems(IEnumerable<IndentItemDto> items) =>
        items.GroupBy(x => new { x.MescGeneralGroupCode, x.GeneralDescription })
            .OrderBy(x => x.Key.MescGeneralGroupCode)
            .Select(x => new IndentGroupedItemsDto(x.Key.MescGeneralGroupCode, x.Key.GeneralDescription,
                x.OrderBy(i => i.MescCode).ToArray())).ToArray();
}
