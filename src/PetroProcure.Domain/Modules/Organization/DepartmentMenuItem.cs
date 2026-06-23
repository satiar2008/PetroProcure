using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Organization;

public sealed class DepartmentMenuItem : AuditableEntity<Guid>
{
    public DepartmentMenuItem(Guid id, DepartmentType departmentType, string title, string route, string? requiredPermission = null, int order = 0)
        : base(id)
    {
        DepartmentType = departmentType;
        Title = string.IsNullOrWhiteSpace(title)
            ? throw new ArgumentException("Title is required.", nameof(title))
            : title.Trim();
        Route = string.IsNullOrWhiteSpace(route)
            ? throw new ArgumentException("Route is required.", nameof(route))
            : route.Trim();
        RequiredPermission = string.IsNullOrWhiteSpace(requiredPermission) ? null : requiredPermission.Trim();
        Order = order;
        IsVisible = true;
    }

    public DepartmentType DepartmentType { get; private set; }

    public string Title { get; private set; }

    public string Route { get; private set; }

    public string? RequiredPermission { get; private set; }

    public int Order { get; private set; }

    public bool IsVisible { get; private set; }
}
