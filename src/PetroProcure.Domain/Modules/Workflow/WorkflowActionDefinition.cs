using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Workflow;

public sealed class WorkflowActionDefinition : Entity<Guid>
{
    public WorkflowActionDefinition(
        Guid id, string code, string title, DepartmentType fromDepartmentType,
        DepartmentType? toDepartmentType, PurchaseFileStatus fromStatus,
        PurchaseFileStatus toStatus, string requiredPermission, bool requiresComment,
        bool isReturnAction, bool isFinalAction, bool isActive = true) : base(id)
    {
        Code = Required(code);
        Title = Required(title);
        FromDepartmentType = fromDepartmentType;
        ToDepartmentType = toDepartmentType;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        RequiredPermission = Required(requiredPermission);
        RequiresComment = requiresComment;
        IsReturnAction = isReturnAction;
        IsFinalAction = isFinalAction;
        IsActive = isActive;
    }

    public string Code { get; private set; }
    public string Title { get; private set; }
    public DepartmentType FromDepartmentType { get; private set; }
    public DepartmentType? ToDepartmentType { get; private set; }
    public PurchaseFileStatus FromStatus { get; private set; }
    public PurchaseFileStatus ToStatus { get; private set; }
    public string RequiredPermission { get; private set; }
    public bool RequiresComment { get; private set; }
    public bool IsReturnAction { get; private set; }
    public bool IsFinalAction { get; private set; }
    public bool IsActive { get; private set; }

    public void Update(string title, string requiredPermission, bool requiresComment, bool isActive)
    {
        Title = Required(title);
        RequiredPermission = Required(requiredPermission);
        RequiresComment = requiresComment;
        IsActive = isActive;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static string Required(string value) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.") : value.Trim();
}
