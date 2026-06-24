using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Suppliers;

namespace PetroProcure.UnitTests.Domain;

public sealed class SupplierTests
{
    [Fact]
    public void Supplier_name_is_required()
    {
        Assert.Throws<ArgumentException>(() =>
            new Supplier(Guid.NewGuid(), "SUP-1", "", SupplierType.Distributor, Guid.NewGuid()));
    }

    [Fact]
    public void Status_changes_work()
    {
        var userId = Guid.NewGuid();
        var supplier = new Supplier(Guid.NewGuid(), "SUP-1", "Supplier", SupplierType.Distributor, userId);

        supplier.Activate(userId);
        Assert.Equal(SupplierStatus.Active, supplier.Status);
        supplier.Deactivate(userId);
        Assert.Equal(SupplierStatus.Inactive, supplier.Status);
        Assert.False(supplier.IsActive);
    }

    [Fact]
    public void Blacklisted_supplier_is_not_eligible_by_default()
    {
        var userId = Guid.NewGuid();
        var supplier = new Supplier(Guid.NewGuid(), "SUP-1", "Supplier", SupplierType.Distributor, userId);

        supplier.Blacklist("Risk", userId);

        Assert.False(supplier.IsEligibleForSelection());
        Assert.True(supplier.IsEligibleForSelection(includeBlacklisted: true, includeInactive: true));
    }

    [Fact]
    public void Only_one_primary_active_contact_is_allowed()
    {
        var supplier = new Supplier(Guid.NewGuid(), "SUP-1", "Supplier", SupplierType.Distributor, Guid.NewGuid());
        var first = new SupplierContact(Guid.NewGuid(), supplier.Id, "First", null, null, null, null, true, null);
        var second = new SupplierContact(Guid.NewGuid(), supplier.Id, "Second", null, null, null, null, true, null);

        supplier.AddContact(first);
        supplier.AddContact(second);

        Assert.False(first.IsPrimary);
        Assert.True(second.IsPrimary);
    }

    [Fact]
    public void Supplier_category_assignment_works()
    {
        var supplier = new Supplier(Guid.NewGuid(), "SUP-1", "Supplier", SupplierType.Distributor, Guid.NewGuid());
        var categoryId = Guid.NewGuid();

        supplier.AddCategoryAssignment(new SupplierCategoryAssignment(Guid.NewGuid(), supplier.Id, categoryId));

        Assert.Contains(supplier.CategoryAssignments, x => x.SupplierCategoryId == categoryId);
    }
}
