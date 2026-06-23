using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Items;

public sealed class UnitOfMeasure : AuditableEntity<Guid>
{
    public UnitOfMeasure(Guid id, string code, string name)
        : base(id)
    {
        Code = string.IsNullOrWhiteSpace(code)
            ? throw new ArgumentException("Code is required.", nameof(code))
            : code.Trim();
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Name is required.", nameof(name))
            : name.Trim();
        IsActive = true;
    }

    public string Code { get; private set; }

    public string Name { get; private set; }

    public bool IsActive { get; private set; }
}
