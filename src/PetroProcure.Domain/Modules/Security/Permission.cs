using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Security;

public sealed class Permission : AuditableEntity<Guid>
{
    public Permission(Guid id, string name, string description)
        : base(id)
    {
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Permission name is required.", nameof(name))
            : name.Trim();
        Description = string.IsNullOrWhiteSpace(description)
            ? throw new ArgumentException("Permission description is required.", nameof(description))
            : description.Trim();
        IsActive = true;
    }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public bool IsActive { get; private set; }
}
