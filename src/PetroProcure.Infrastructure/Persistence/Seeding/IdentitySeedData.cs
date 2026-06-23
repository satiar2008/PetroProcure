using Microsoft.AspNetCore.Identity;
using PetroProcure.Application.Security;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Domain.Modules.Security;
using PetroProcure.Infrastructure.Identity;

namespace PetroProcure.Infrastructure.Persistence.Seeding;

public static class IdentitySeedData
{
    public const string DefaultAdminUserName = "admin";
    public const string DefaultAdminEmail = "admin@petroprocure.local";

    public static readonly Guid DefaultAdminUserId = StableGuid.Create("identity:user:admin");
    public static readonly Guid DefaultAdminProfileId = StableGuid.Create("org:user-profile:admin");

    public static readonly IReadOnlyDictionary<string, Guid> RoleIds = ApplicationRoles.All
        .ToDictionary(role => role, role => StableGuid.Create($"identity:role:{role}"));

    public static readonly IReadOnlyDictionary<string, Guid> PermissionIds = ApplicationPermissions.All
        .ToDictionary(permission => permission, permission => StableGuid.Create($"security:permission:{permission}"));

    public static readonly ApplicationUserProfile AdminProfile = new(
        DefaultAdminProfileId,
        "مدیر سامانه",
        DefaultAdminEmail);

    public static readonly ApplicationUser AdminUser = CreateAdminUser();

    public static readonly IdentityRole<Guid>[] Roles = ApplicationRoles.All
        .Select(role => new IdentityRole<Guid>
        {
            Id = RoleIds[role],
            Name = role,
            NormalizedName = role.ToUpperInvariant(),
            ConcurrencyStamp = StableGuid.Create($"identity:role-stamp:{role}").ToString()
        })
        .ToArray();

    public static readonly IdentityUserRole<Guid> AdminUserRole = new()
    {
        UserId = DefaultAdminUserId,
        RoleId = RoleIds[ApplicationRoles.SystemAdmin]
    };

    public static readonly Permission[] Permissions = ApplicationPermissions.All
        .Select(permission => new Permission(
            PermissionIds[permission],
            permission,
            permission))
        .ToArray();

    public static readonly RolePermission[] RolePermissions = CreateRolePermissions();

    public static readonly DepartmentMenuItem[] DepartmentMenuItems =
    [
        new(StableGuid.Create("menu:purchase:files"), DepartmentType.PurchaseDepartment, "پرونده‌های خرید", "/purchase-files", ApplicationPermissions.PurchaseFileView, 1),
        new(StableGuid.Create("menu:orders:indents"), DepartmentType.OrdersAndInventoryControl, "درخواست‌های خرید", "/indents", ApplicationPermissions.IndentView, 1),
        new(StableGuid.Create("menu:warehouse:tasks"), DepartmentType.Warehouse, "عملیات انبار", "/warehouse", ApplicationPermissions.WarehouseView, 1),
        new(StableGuid.Create("menu:applicant:requests"), DepartmentType.Applicant, "درخواست‌های من", "/applicant/requests", ApplicationPermissions.IndentCreate, 1),
        new(StableGuid.Create("menu:tender:commission"), DepartmentType.TenderCommission, "کمیسیون مناقصه", "/tenders", ApplicationPermissions.TenderView, 1)
    ];

    public static readonly Department[] MainDepartments =
    [
        new(SeedDataIds.PurchaseDepartmentId, "واحد خرید", DepartmentType.PurchaseDepartment),
        new(SeedDataIds.OrdersAndInventoryControlId, "سفارشات و کنترل موجودی", DepartmentType.OrdersAndInventoryControl),
        new(SeedDataIds.WarehouseId, "انبار", DepartmentType.Warehouse),
        new(SeedDataIds.ApplicantId, "متقاضی", DepartmentType.Applicant),
        new(SeedDataIds.TenderCommissionId, "کمیسیون مناقصه", DepartmentType.TenderCommission)
    ];

    private static ApplicationUser CreateAdminUser()
    {
        var user = new ApplicationUser
        {
            Id = DefaultAdminUserId,
            UserName = DefaultAdminUserName,
            NormalizedUserName = DefaultAdminUserName.ToUpperInvariant(),
            Email = DefaultAdminEmail,
            NormalizedEmail = DefaultAdminEmail.ToUpperInvariant(),
            EmailConfirmed = true,
            SecurityStamp = StableGuid.Create("identity:user-security-stamp:admin").ToString(),
            ConcurrencyStamp = StableGuid.Create("identity:user-concurrency-stamp:admin").ToString(),
            UserProfileId = DefaultAdminProfileId
        };

        return user;
    }

    private static RolePermission[] CreateRolePermissions()
    {
        var definitions = new Dictionary<string, string[]>
        {
            [ApplicationRoles.SystemAdmin] = ApplicationPermissions.All,
            [ApplicationRoles.PurchaseManager] =
            [
                ApplicationPermissions.PurchaseFileCreate,
                ApplicationPermissions.PurchaseFileView,
                ApplicationPermissions.PurchaseFileEdit,
                ApplicationPermissions.PurchaseFileSendToDepartment,
                ApplicationPermissions.PurchaseFileClose,
                ApplicationPermissions.PurchaseFileArchive,
                ApplicationPermissions.IndentView,
                ApplicationPermissions.ItemView,
                ApplicationPermissions.TenderView,
                ApplicationPermissions.ReportView,
                ApplicationPermissions.ReportPrint,
                ApplicationPermissions.ReportExportPdf
            ],
            [ApplicationRoles.PurchaseExpert] =
            [
                ApplicationPermissions.PurchaseFileCreate,
                ApplicationPermissions.PurchaseFileView,
                ApplicationPermissions.PurchaseFileEdit,
                ApplicationPermissions.PurchaseFileSendToDepartment,
                ApplicationPermissions.IndentView,
                ApplicationPermissions.ItemView
            ],
            [ApplicationRoles.OrdersManager] =
            [
                ApplicationPermissions.IndentCreate,
                ApplicationPermissions.IndentView,
                ApplicationPermissions.IndentApprove,
                ApplicationPermissions.IndentSendToPurchase,
                ApplicationPermissions.ItemView
            ],
            [ApplicationRoles.OrdersUser] =
            [
                ApplicationPermissions.IndentCreate,
                ApplicationPermissions.IndentView,
                ApplicationPermissions.IndentSendToPurchase,
                ApplicationPermissions.ItemView
            ],
            [ApplicationRoles.WarehouseManager] =
            [
                ApplicationPermissions.WarehouseView,
                ApplicationPermissions.WarehouseReceive,
                ApplicationPermissions.WarehouseIssue,
                ApplicationPermissions.ItemView
            ],
            [ApplicationRoles.WarehouseUser] =
            [
                ApplicationPermissions.WarehouseView,
                ApplicationPermissions.WarehouseReceive,
                ApplicationPermissions.ItemView
            ],
            [ApplicationRoles.ApplicantUser] =
            [
                ApplicationPermissions.IndentCreate,
                ApplicationPermissions.IndentView,
                ApplicationPermissions.PurchaseFileView,
                ApplicationPermissions.ItemView
            ],
            [ApplicationRoles.TenderCommissionManager] =
            [
                ApplicationPermissions.TenderCreate,
                ApplicationPermissions.TenderView,
                ApplicationPermissions.TenderEvaluate,
                ApplicationPermissions.TenderApproveWinner,
                ApplicationPermissions.PurchaseFileView
            ],
            [ApplicationRoles.TenderCommissionMember] =
            [
                ApplicationPermissions.TenderView,
                ApplicationPermissions.TenderEvaluate,
                ApplicationPermissions.PurchaseFileView
            ],
            [ApplicationRoles.ReportViewer] =
            [
                ApplicationPermissions.ReportView,
                ApplicationPermissions.ReportPrint,
                ApplicationPermissions.ReportExportPdf
            ],
            [ApplicationRoles.AiAgentUser] =
            [
                ApplicationPermissions.AiAgentUse,
                ApplicationPermissions.AiAgentEvaluatePurchaseRules,
                ApplicationPermissions.PurchaseFileView
            ]
        };

        return definitions
            .SelectMany(role => role.Value.Select(permission => new RolePermission(
                StableGuid.Create($"security:role-permission:{role.Key}:{permission}"),
                RoleIds[role.Key],
                PermissionIds[permission])))
            .ToArray();
    }
}
