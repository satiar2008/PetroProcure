using PetroProcure.Contracts.V1.Workflow;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Web.Services;

public static class InboxUiPolicy
{
    public static bool CanView(Func<string, bool> has) => has("PurchaseFile.View");
    public static bool CanManage(Func<string, bool> has) => has("PurchaseFile.SendToDepartment");
    public static bool CanAssignOther(Func<string, bool> has) =>
        CanManage(has) && has("Admin.ManageUsers");
    public static bool CanAssignToSelf(InboxTaskDto task, Guid userId, Func<string, bool> has) =>
        CanManage(has) && task.Status is WorkflowStatus.Pending or WorkflowStatus.InProgress
        && task.AssignedUserId != userId;
    public static bool CanComplete(InboxTaskDto task, Guid userId, Func<string, bool> has) =>
        CanManage(has) && task.Status is WorkflowStatus.Pending or WorkflowStatus.InProgress
        && (!task.AssignedUserId.HasValue || task.AssignedUserId == userId);
    public static bool HasDepartmentAccess(InboxTaskDto task, IEnumerable<Guid> departmentIds, bool isAdmin) =>
        isAdmin || departmentIds.Contains(task.AssignedDepartmentId);
    public static string EntityType(InboxTaskDto task) =>
        task.PurchaseFileId.HasValue ? "پرونده خرید" : task.IndentId.HasValue ? "تقاضای خرید" : "گردش کار";
    public static string RelatedLink(InboxTaskDto task) =>
        task.PurchaseFileId.HasValue ? $"/purchase/files/{task.PurchaseFileId}"
        : task.IndentId.HasValue ? $"/orders/indents/{task.IndentId}" : "#";
    public static string StatusText(WorkflowStatus status) => status switch
    {
        WorkflowStatus.Pending => "در انتظار",
        WorkflowStatus.InProgress => "در حال انجام",
        WorkflowStatus.Completed => "تکمیل‌شده",
        WorkflowStatus.Returned => "برگشت‌شده",
        WorkflowStatus.Approved => "تأییدشده",
        WorkflowStatus.Rejected => "ردشده",
        WorkflowStatus.Cancelled => "لغوشده",
        _ => "شروع‌نشده"
    };
}
