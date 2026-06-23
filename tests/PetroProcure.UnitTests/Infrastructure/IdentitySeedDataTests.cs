using PetroProcure.Domain.Enums;
using PetroProcure.Infrastructure.Persistence.Seeding;

namespace PetroProcure.UnitTests.Infrastructure;

public sealed class IdentitySeedDataTests
{
    [Fact]
    public void MainDepartments_ContainRequiredDepartmentTypes()
    {
        var departmentTypes = IdentitySeedData.MainDepartments
            .Select(department => department.Type)
            .ToArray();

        Assert.Contains(DepartmentType.PurchaseDepartment, departmentTypes);
        Assert.Contains(DepartmentType.OrdersAndInventoryControl, departmentTypes);
        Assert.Contains(DepartmentType.Warehouse, departmentTypes);
        Assert.Contains(DepartmentType.Applicant, departmentTypes);
        Assert.Contains(DepartmentType.TenderCommission, departmentTypes);
    }
}
