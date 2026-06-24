using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Suppliers;

public sealed class SupplierContact : Entity<Guid>
{
    private SupplierContact()
        : base(Guid.Empty)
    {
        FullName = string.Empty;
    }

    public SupplierContact(
        Guid id,
        Guid supplierId,
        string fullName,
        string? position,
        string? phone,
        string? mobile,
        string? email,
        bool isPrimary,
        string? description)
        : base(id)
    {
        SupplierId = supplierId;
        FullName = Required(fullName, nameof(fullName));
        Position = Trim(position);
        Phone = Trim(phone);
        Mobile = Trim(mobile);
        Email = Trim(email);
        IsPrimary = isPrimary;
        IsActive = true;
        Description = Trim(description);
    }

    public Guid SupplierId { get; private set; }
    public Supplier? Supplier { get; private set; }
    public string FullName { get; private set; }
    public string? Position { get; private set; }
    public string? Phone { get; private set; }
    public string? Mobile { get; private set; }
    public string? Email { get; private set; }
    public bool IsPrimary { get; private set; }
    public bool IsActive { get; private set; }
    public string? Description { get; private set; }

    public void Update(string fullName, string? position, string? phone, string? mobile, string? email, bool isPrimary, string? description)
    {
        FullName = Required(fullName, nameof(fullName));
        Position = Trim(position);
        Phone = Trim(phone);
        Mobile = Trim(mobile);
        Email = Trim(email);
        IsPrimary = isPrimary;
        Description = Trim(description);
    }

    public void SetPrimary(bool isPrimary) => IsPrimary = isPrimary;
    public void Deactivate()
    {
        IsActive = false;
        IsPrimary = false;
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
