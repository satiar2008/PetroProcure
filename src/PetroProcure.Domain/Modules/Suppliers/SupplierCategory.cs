using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Suppliers;

public sealed class SupplierCategory : Entity<Guid>
{
    private SupplierCategory()
        : base(Guid.Empty)
    {
        Code = string.Empty;
        Title = string.Empty;
    }

    public SupplierCategory(Guid id, string code, string title, string? description = null)
        : base(id)
    {
        Code = Required(code, nameof(code));
        Title = Required(title, nameof(title));
        Description = Trim(description);
        IsActive = true;
    }

    public string Code { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    public void Update(string code, string title, string? description)
    {
        Code = Required(code, nameof(code));
        Title = Required(title, nameof(title));
        Description = Trim(description);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
