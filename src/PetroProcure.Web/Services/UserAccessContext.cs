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
    IReadOnlyList<DepartmentAccess> Panels { get; }
    DepartmentAccess CurrentPanel { get; }
    IReadOnlySet<string> Permissions { get; }
    IReadOnlyList<NavigationAccess> NavigationItems { get; }
    event Action? Changed;
    void SwitchDepartment(string key);
    void SwitchPanel(string key);
    bool HasPermission(string permission);
}

public sealed class UserAccessContext : IUserAccessContext, IDisposable
{
    private const string DepartmentStorageKey = "petroprocure.department";
    private const string PanelStorageKey = "petroprocure.panel";
    private readonly IAuthService _auth;
    private readonly ProtectedLocalStorage _storage;
    private DepartmentAccess? _current;
    private DepartmentAccess? _currentPanel;

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
    public IReadOnlyList<DepartmentAccess> Panels { get; private set; } = [];
    public DepartmentAccess CurrentPanel => _currentPanel
        ?? Panels.FirstOrDefault()
        ?? CurrentDepartment;
    public IReadOnlySet<string> Permissions { get; private set; } = new HashSet<string>();

    public IReadOnlyList<NavigationAccess> NavigationItems { get; } =
    [
        new("داشبورد خرید", "/purchase", Icons.Material.Filled.Dashboard, "purchase", "PurchaseFile.View"),
        new("پرونده‌های خرید", "/purchase/files", Icons.Material.Filled.FolderOpen, "purchase", "PurchaseFile.View"),
        new("استعلام‌ها", "/purchase/inquiries", Icons.Material.Filled.RequestQuote, "purchase", "Inquiry.View"),
        new("مناقصات", "/purchase/tenders", Icons.Material.Filled.Gavel, "purchase", "Tender.View"),
        new("فهرست مناقصات", "/purchase/tenders", Icons.Material.Filled.FormatListBulleted, "purchase", "Tender.View"),
        new("تأمین‌کنندگان", "/purchase/suppliers", Icons.Material.Filled.Business, "purchase", "Supplier.View"),
        new("دسته‌بندی تأمین‌کنندگان", "/purchase/suppliers/categories", Icons.Material.Filled.Category, "purchase", "Supplier.View"),
        new("کارتابل من", "/inbox/my", Icons.Material.Filled.Inbox, null, null),
        new("کارتابل واحد", "/inbox/department", Icons.Material.Filled.GroupWork, null, null),
        new("داشبورد سفارشات", "/orders", Icons.Material.Filled.SpaceDashboard, "orders", "Orders.ViewDashboard"),
        new("کنترل موجودی", "/orders/inventory-control", Icons.Material.Filled.Inventory2, "orders", "Orders.ViewInventory"),
        new("نیازهای کالا", "/orders/material-needs", Icons.Material.Filled.Assignment, "orders", "Orders.CreateMaterialNeed"),
        new("هشدارهای کمبود", "/orders/shortage-alerts", Icons.Material.Filled.WarningAmber, "orders", "Orders.ManageShortageAlerts"),
        new("درخواست‌های خرید", "/orders/indents", Icons.Material.Filled.ReceiptLong, "orders", "Indent.View"),
        new("کاتالوگ MESC", "/orders/mesc", Icons.Material.Filled.Category, "orders", "Item.View"),
        new("گروه‌های عمومی کالا", "/orders/mesc/groups", Icons.Material.Filled.AccountTree, "orders", "Item.View"),
        new("اقلام کالا", "/orders/mesc/items", Icons.Material.Filled.Inventory, "orders", "Item.View"),
        new("تنظیمات کاتالوگ کالا", "/orders/mesc", Icons.Material.Filled.SettingsSuggest, "admin", "Admin.ManageSettings"),
        new("داشبورد انبار", "/warehouse", Icons.Material.Filled.Warehouse, "warehouse", "Warehouse.View"),
        new("داشبورد متقاضی", "/applicant", Icons.Material.Filled.PersonPin, "applicant", "Indent.View"),
        new("کمیسیون مناقصه", "/tender-commission", Icons.Material.Filled.Gavel, "tender-commission", "Tender.View"),
        new("جلسات کمیسیون", "/tender-commission/sessions", Icons.Material.Filled.Groups, "tender-commission", "Commission.View"),
        new("پرونده‌های مناقصه", "/purchase/tenders", Icons.Material.Filled.FolderSpecial, "tender-commission", "Tender.View"),
        new("تصمیمات کمیسیون", "/tender-commission/sessions", Icons.Material.Filled.FactCheck, "tender-commission", "Commission.View"),
        new("مدیریت سامانه", "/admin", Icons.Material.Filled.AdminPanelSettings, "admin", "Admin.ManageUsers"),
        new("کاربران", "/admin/users", Icons.Material.Filled.People, "admin", "Admin.ManageUsers"),
        new("نقش‌ها", "/admin/roles", Icons.Material.Filled.Badge, "admin", "Admin.ManageRoles"),
        new("واحدهای سازمانی", "/admin/departments", Icons.Material.Filled.Apartment, "admin", "Admin.ManageDepartments"),
        new("مجوزها", "/admin/permissions", Icons.Material.Filled.VerifiedUser, "admin", "Admin.ManageRoles"),
        new("ماتریس گردش کار", "/admin/workflow-matrix", Icons.Material.Filled.AccountTree, "admin", "Admin.ManageSettings"),
        new("تنظیمات سیستم", "/admin/settings", Icons.Material.Filled.Settings, "admin", "Admin.ManageSettings"),
        new("گزارش ممیزی ادمین", "/admin/audit-log", Icons.Material.Filled.History, "admin", "Admin.ManageSettings"),
        new("گزارش‌ها", "/reports", Icons.Material.Filled.Assessment, "reports", "Report.View"),
        new("دستیار هوشمند", "/ai", Icons.Material.Filled.AutoAwesome, "ai", "AiAgent.Use")
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

    public void SwitchPanel(string key)
    {
        var panel = Panels.FirstOrDefault(item => item.Key == key);
        if (panel is null || panel == _currentPanel) return;
        _currentPanel = panel;
        _ = _storage.SetAsync(PanelStorageKey, panel.Key);

        var matchingDepartment = Departments.FirstOrDefault(item => item.Key == key);
        if (matchingDepartment is not null)
        {
            _current = matchingDepartment;
            _ = _storage.SetAsync(DepartmentStorageKey, matchingDepartment.Id);
        }

        Changed?.Invoke();
    }

    public bool HasPermission(string permission) =>
        _auth.CurrentUser?.IsSystemAdmin == true || Permissions.Contains(permission);

    private void Refresh()
    {
        var user = _auth.CurrentUser;
        Departments = user?.Departments.Select(MapDepartment).ToArray() ?? [];
        Permissions = new HashSet<string>(user?.Permissions ?? [], StringComparer.OrdinalIgnoreCase);
        Panels = BuildPanels();
        if (_current is null || Departments.All(x => x.Id != _current.Id))
            _current = Departments.FirstOrDefault();
        if (_currentPanel is null || Panels.All(x => x.Key != _currentPanel.Key))
            _currentPanel = Panels.FirstOrDefault(x => x.Key == _current?.Key) ?? Panels.FirstOrDefault();
        _ = RestoreSelectionAsync();
        Changed?.Invoke();
    }

    private async Task RestoreSelectionAsync()
    {
        try
        {
            var stored = await _storage.GetAsync<Guid>(DepartmentStorageKey);
            if (stored.Success)
            {
                var department = Departments.FirstOrDefault(x => x.Id == stored.Value);
                if (department is not null && department != _current)
                    _current = department;
            }

            var storedPanel = await _storage.GetAsync<string>(PanelStorageKey);
            if (storedPanel.Success)
            {
                var panel = Panels.FirstOrDefault(x => x.Key == storedPanel.Value);
                if (panel is not null && panel != _currentPanel)
                    _currentPanel = panel;
            }

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

    private IReadOnlyList<DepartmentAccess> BuildPanels()
    {
        if (_auth.CurrentUser?.IsSystemAdmin == true)
            return SystemAdminPanels
                .Select(panel => Departments.FirstOrDefault(department => department.Key == panel.Key) ?? panel)
                .ToArray();

        return Departments;
    }

    private static readonly DepartmentAccess[] SystemAdminPanels =
    [
        new(Guid.Empty, "purchase", "واحد خرید", "/purchase", Icons.Material.Filled.ShoppingCart),
        new(Guid.Empty, "orders", "سفارشات و کنترل موجودی", "/orders", Icons.Material.Filled.Inventory2),
        new(Guid.Empty, "warehouse", "انبار", "/warehouse", Icons.Material.Filled.Warehouse),
        new(Guid.Empty, "applicant", "متقاضی", "/applicant", Icons.Material.Filled.PersonPin),
        new(Guid.Empty, "tender-commission", "کمیسیون مناقصه", "/tender-commission", Icons.Material.Filled.Gavel),
        new(Guid.Empty, "admin", "مدیریت سامانه", "/admin", Icons.Material.Filled.AdminPanelSettings),
        new(Guid.Empty, "reports", "گزارش‌ها", "/reports", Icons.Material.Filled.Assessment),
        new(Guid.Empty, "ai", "دستیار هوشمند", "/ai", Icons.Material.Filled.AutoAwesome)
    ];

    public void Dispose() => _auth.AuthenticationChanged -= Refresh;
}
