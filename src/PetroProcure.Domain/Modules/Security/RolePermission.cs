using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Security;

public sealed class RolePermission : AuditableEntity<Guid>
{
    public RolePermission(Guid id, Guid roleId, Guid permissionId)
        : base(id)
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }

    public Guid RoleId { get; private set; }

    public Guid PermissionId { get; private set; }
}
