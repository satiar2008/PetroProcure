using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Organization;

public sealed class Department : AuditableEntity<Guid>
{
    public Department(Guid id, string name, DepartmentType type)
        : base(id)
    {
        Name = GuardRequired(name, nameof(name));
        Type = type;
        IsActive = true;
    }

    public string Name { get; private set; }

    public DepartmentType Type { get; private set; }

    public bool IsActive { get; private set; }

    public void Rename(string name)
    {
        Name = GuardRequired(name, nameof(name));
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static string GuardRequired(string value, string parameterName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value is required.", parameterName)
            : value.Trim();
    }
}
