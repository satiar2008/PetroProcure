using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MudBlazor;
using PetroProcure.Contracts.V1.Organization;
using PetroProcure.Web.Services.Auth;

namespace PetroProcure.Web.Services;

public sealed record DepartmentAccess(Guid Id, string Key, string Title, string Route, string Icon);
public sealed record NavigationAccess(string Title, string Route, string Icon, string? DepartmentKey, string? Permission);

public interface IUserAccessContext
{
    string DisplayName { get; }
    string RoleTitle { get; }
    IReadOnlyList<DepartmentAccess> Departments { get; }
    DepartmentAccess CurrentDepartment { get; }
    IReadOnlySet<string> Permissions { get; }
    IReadOnlyList<NavigationAccess> NavigationItems { get; }
    event Action? Changed;
    void SwitchDepartment(string key);
    bool HasPermission(string permission);
}

public sealed class UserAccessContext : IUserAccessContext, IDisposable
{
    private const string DepartmentStorageKey = "petroprocure.department";
    private readonly IAuthService _auth;
    private readonly ProtectedLocalStorage _storage;
    private DepartmentAccess? _current;

    public UserAccessContext(IAuthService auth, ProtectedLocalStorage storage)
    {
        _auth = auth;
        _storage = storage;
        _auth.AuthenticationChanged += Refresh;
        Refresh();
    }

    public string DisplayName => _auth.CurrentUser?.DisplayName
        ?? _auth.CurrentUser?.UserName
        ?? "کاربر";
    public string RoleTitle => _auth.CurrentUser?.Roles.FirstOrDefault() ?? string.Empty;
    public IReadOnlyList<DepartmentAccess> Departments { get; private set; } = [];
    public DepartmentAccess CurrentDepartment => _current
        ?? new(Guid.Empty, "none", "بدون واحد سازمانی", "/", Icons.Material.Filled.Apartment);
    public IReadOnlySet<string> Permissions { get; private set; } = new HashSet<string>();

    public IReadOnlyList<NavigationAccess> NavigationItems { get; } =
    [
        new("داشبورد خرید", "/purchase", Icons.Material.Filled.Dashboard, "purchase", "PurchaseFile.View"),
        new("پرونده‌های خرید", "/purchase/files", Icons.Material.Filled.FolderOpen, "purchase", "PurchaseFile.View"),
        new("کارتابل من", "/inbox/my", Icons.Material.Filled.Inbox, null, "PurchaseFile.View"),
        new("کارتابل واحد", "/inbox/department", Icons.Material.Filled.GroupWork, null, "PurchaseFile.View"),
        new("داشبورد سفارشات", "/orders", Icons.Material.Filled.SpaceDashboard, "orders", "Indent.View"),
        new("درخواست‌های خرید", "/orders/indents", Icons.Material.Filled.ReceiptLong, "orders", "Indent.View"),
        new("کاتالوگ MESC", "/orders/mesc", Icons.Material.Filled.Category, "orders", "Item.View"),
        new("گروه‌های عمومی کالا", "/orders/mesc/groups", Icons.Material.Filled.AccountTree, "orders", "Item.View"),
        new("اقلام کالا", "/orders/mesc/items", Icons.Material.Filled.Inventory, "orders", "Item.View"),
        new("تنظیمات کاتالوگ کالا", "/orders/mesc", Icons.Material.Filled.SettingsSuggest, null, "Admin.ManageSettings"),
        new("داشبورد انبار", "/warehouse", Icons.Material.Filled.Warehouse, "warehouse", "Warehouse.View"),
        new("داشبورد متقاضی", "/applicant", Icons.Material.Filled.PersonPin, "applicant", "Indent.View"),
        new("کمیسیون مناقصه", "/tender-commission", Icons.Material.Filled.Gavel, "tender-commission", "Tender.View"),
        new("مدیریت سامانه", "/admin", Icons.Material.Filled.AdminPanelSettings, null, "Admin.ManageUsers"),
        new("کاربران", "/admin/users", Icons.Material.Filled.People, null, "Admin.ManageUsers"),
        new("نقش‌ها", "/admin/roles", Icons.Material.Filled.Badge, null, "Admin.ManageRoles"),
        new("واحدهای سازمانی", "/admin/departments", Icons.Material.Filled.Apartment, null, "Admin.ManageDepartments"),
        new("مجوزها", "/admin/permissions", Icons.Material.Filled.VerifiedUser, null, "Admin.ManageRoles"),
        new("ماتریس گردش کار", "/admin/workflow-matrix", Icons.Material.Filled.AccountTree, null, "Admin.ManageSettings"),
        new("تنظیمات سیستم", "/admin/settings", Icons.Material.Filled.Settings, null, "Admin.ManageSettings"),
        new("گزارش ممیزی ادمین", "/admin/audit-log", Icons.Material.Filled.History, null, "Admin.ManageSettings"),
        new("گزارش‌ها", "/reports", Icons.Material.Filled.Assessment, null, "Report.View"),
        new("دستیار هوشمند", "/ai", Icons.Material.Filled.AutoAwesome, null, "AiAgent.Use")
    ];

    public event Action? Changed;

    public void SwitchDepartment(string key)
    {
        var department = Departments.FirstOrDefault(item => item.Key == key);
        if (department is null || department == _current) return;
        _current = department;
        _ = _storage.SetAsync(DepartmentStorageKey, department.Id);
        Changed?.Invoke();
    }

    public bool HasPermission(string permission) =>
        _auth.CurrentUser?.IsSystemAdmin == true || Permissions.Contains(permission);

    private void Refresh()
    {
        var user = _auth.CurrentUser;
        Departments = user?.Departments.Select(MapDepartment).ToArray() ?? [];
        Permissions = new HashSet<string>(user?.Permissions ?? [], StringComparer.OrdinalIgnoreCase);
        if (_current is null || Departments.All(x => x.Id != _current.Id))
            _current = Departments.FirstOrDefault();
        _ = RestoreSelectionAsync();
        Changed?.Invoke();
    }

    private async Task RestoreSelectionAsync()
    {
        try
        {
            var stored = await _storage.GetAsync<Guid>(DepartmentStorageKey);
            if (!stored.Success) return;
            var department = Departments.FirstOrDefault(x => x.Id == stored.Value);
            if (department is null || department == _current) return;
            _current = department;
            Changed?.Invoke();
        }
        catch (InvalidOperationException)
        {
            // Browser storage is not available during static rendering.
        }
    }

    private static DepartmentAccess MapDepartment(DepartmentDto department)
    {
        var (key, route, icon) = department.Type switch
        {
            "PurchaseDepartment" => ("purchase", "/purchase", Icons.Material.Filled.ShoppingCart),
            "OrdersAndInventoryControl" => ("orders", "/orders", Icons.Material.Filled.Inventory2),
            "Warehouse" => ("warehouse", "/warehouse", Icons.Material.Filled.Warehouse),
            "Applicant" => ("applicant", "/applicant", Icons.Material.Filled.PersonPin),
            "TenderCommission" => ("tender-commission", "/tender-commission", Icons.Material.Filled.Gavel),
            _ => ($"department-{department.Id:N}", "/", Icons.Material.Filled.Apartment)
        };
        return new DepartmentAccess(department.Id, key, department.Name, route, icon);
    }

    public void Dispose() => _auth.AuthenticationChanged -= Refresh;
}
