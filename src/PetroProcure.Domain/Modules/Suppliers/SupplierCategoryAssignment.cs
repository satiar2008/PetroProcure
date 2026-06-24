using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Suppliers;

public sealed class SupplierCategoryAssignment : Entity<Guid>
{
    private SupplierCategoryAssignment()
        : base(Guid.Empty)
    {
    }

    public SupplierCategoryAssignment(Guid id, Guid supplierId, Guid supplierCategoryId)
        : base(id)
    {
        SupplierId = supplierId;
        SupplierCategoryId = supplierCategoryId;
    }

    public Guid SupplierId { get; private set; }
    public Supplier? Supplier { get; private set; }
    public Guid SupplierCategoryId { get; private set; }
    public SupplierCategory? SupplierCategory { get; private set; }
}
