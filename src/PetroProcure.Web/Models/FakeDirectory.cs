namespace PetroProcure.Web.Models;

public static class FakeDirectory
{
    public static readonly Guid PurchaseDepartmentId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid OrdersDepartmentId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid WarehouseDepartmentId = Guid.Parse("10000000-0000-0000-0000-000000000003");
    public static readonly Guid ApplicantDepartmentId = Guid.Parse("10000000-0000-0000-0000-000000000004");
    public static readonly Guid TenderDepartmentId = Guid.Parse("10000000-0000-0000-0000-000000000005");
    public static readonly (Guid Id, string Name)[] Departments =
    [
        (PurchaseDepartmentId, "واحد خرید"),
        (OrdersDepartmentId, "سفارشات و کنترل موجودی"),
        (WarehouseDepartmentId, "انبار"),
        (ApplicantDepartmentId, "متقاضی"),
        (TenderDepartmentId, "کمیسیون مناقصه")
    ];

    public static string DepartmentName(Guid id) =>
        Departments.FirstOrDefault(x => x.Id == id).Name ?? "واحد نامشخص";

    public static Guid UnitId(string code) => code.ToUpperInvariant() switch
    {
        "M" => Guid.Parse("20000000-0000-0000-0000-000000000002"),
        "KG" => Guid.Parse("20000000-0000-0000-0000-000000000003"),
        "L" => Guid.Parse("20000000-0000-0000-0000-000000000004"),
        "PKG" => Guid.Parse("20000000-0000-0000-0000-000000000005"),
        "DEV" => Guid.Parse("20000000-0000-0000-0000-000000000006"),
        _ => Guid.Parse("20000000-0000-0000-0000-000000000001")
    };
}
