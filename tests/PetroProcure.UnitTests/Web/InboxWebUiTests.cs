using Microsoft.AspNetCore.Components;
using PetroProcure.Contracts.V1.Workflow;
using PetroProcure.Domain.Enums;
using PetroProcure.Web.Services;

namespace PetroProcure.UnitTests.Web;

public sealed class InboxWebUiTests
{
    [Fact]
    public void My_inbox_page_is_registered() =>
        AssertRoute("PetroProcure.Web.Components.Pages.Inbox.MyInbox", "/inbox/my");

    [Fact]
    public void Department_inbox_page_is_registered() =>
        AssertRoute("PetroProcure.Web.Components.Pages.Inbox.DepartmentInbox", "/inbox/department");

    [Fact]
    public void Notification_bell_component_is_available() =>
        Assert.NotNull(typeof(InboxUiPolicy).Assembly.GetType(
            "PetroProcure.Web.Components.Inbox.InboxNotificationBell"));

    [Fact]
    public void Inbox_is_available_for_workflow_department_permissions()
    {
        Assert.True(InboxUiPolicy.CanView(permission => permission == "Warehouse.View"));
        Assert.True(InboxUiPolicy.CanManage(permission => permission == "Warehouse.View"));
    }

    [Fact]
    public void User_only_has_access_to_assigned_departments()
    {
        var department = Guid.NewGuid();
        Assert.True(InboxUiPolicy.HasDepartmentAccess(Task(department), [department], false));
        Assert.False(InboxUiPolicy.HasDepartmentAccess(Task(department), [Guid.NewGuid()], false));
    }

    [Fact]
    public void Assign_to_self_requires_permission_and_open_task()
    {
        var user = Guid.NewGuid();
        Assert.True(InboxUiPolicy.CanAssignToSelf(Task(Guid.NewGuid()), user, _ => true));
        Assert.False(InboxUiPolicy.CanAssignToSelf(Task(Guid.NewGuid(), WorkflowStatus.Completed), user, _ => true));
    }

    [Fact]
    public void Complete_action_is_hidden_for_task_assigned_to_another_user()
    {
        var task = Task(Guid.NewGuid(), assignedUserId: Guid.NewGuid());
        Assert.False(InboxUiPolicy.CanComplete(task, Guid.NewGuid(), _ => true));
    }

    [Fact]
    public void Unauthorized_actions_are_hidden()
    {
        var task = Task(Guid.NewGuid());
        Assert.False(InboxUiPolicy.CanAssignToSelf(task, Guid.NewGuid(), _ => false));
        Assert.False(InboxUiPolicy.CanComplete(task, Guid.NewGuid(), _ => false));
    }

    [Fact]
    public void Task_detail_links_to_related_entity()
    {
        var fileId = Guid.NewGuid();
        var indentId = Guid.NewGuid();
        Assert.Equal($"/purchase/files/{fileId}", InboxUiPolicy.RelatedLink(Task(Guid.NewGuid(), purchaseFileId: fileId)));
        Assert.Equal($"/orders/indents/{indentId}", InboxUiPolicy.RelatedLink(Task(Guid.NewGuid(), indentId: indentId)));
    }

    private static InboxTaskDto Task(Guid departmentId, WorkflowStatus status = WorkflowStatus.Pending,
        Guid? assignedUserId = null, Guid? purchaseFileId = null, Guid? indentId = null) =>
        new(Guid.NewGuid(), Guid.NewGuid(), purchaseFileId, indentId, departmentId, assignedUserId,
            "کار آزمون", null, status, null, DateTime.UtcNow, null);

    private static void AssertRoute(string typeName, string route)
    {
        var type = typeof(InboxUiPolicy).Assembly.GetType(typeName);
        Assert.NotNull(type);
        Assert.Contains(type!.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>(),
            attribute => attribute.Template == route);
    }
}
