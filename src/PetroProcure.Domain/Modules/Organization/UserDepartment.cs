using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Organization;

public sealed class UserDepartment : AuditableEntity<Guid>
{
    public UserDepartment(Guid id, Guid userProfileId, Guid departmentId, bool isPrimary = false)
        : base(id)
    {
        UserProfileId = userProfileId;
        DepartmentId = departmentId;
        IsPrimary = isPrimary;
    }

    public Guid UserProfileId { get; private set; }

    public Guid DepartmentId { get; private set; }

    public bool IsPrimary { get; private set; }
}
