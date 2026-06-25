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
    public const string DefaultAdminPassword = "Admin@123456";
    public const string DefaultAdminPasswordHash = "AQAAAAIAAYagAAAAEP4Pkm7AMVlZbxtV8i/536d40zis6sJQNNNupy7R4tqFLRzt2sPJTRQwhfqlkWaq3g==";

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
        new(StableGuid.Create("menu:purchase:suppliers"), DepartmentType.PurchaseDepartment, "تأمین‌کنندگان", "/purchase/suppliers", ApplicationPermissions.SupplierView, 2),
        new(StableGuid.Create("menu:purchase:purchase-orders"), DepartmentType.PurchaseDepartment, "سفارش‌های خرید", "/purchase/purchase-orders", ApplicationPermissions.PurchaseOrderView, 3),
        new(StableGuid.Create("menu:orders:dashboard"), DepartmentType.OrdersAndInventoryControl, "داشبورد سفارشات", "/orders", ApplicationPermissions.OrdersViewDashboard, 1),
        new(StableGuid.Create("menu:orders:inventory"), DepartmentType.OrdersAndInventoryControl, "کنترل موجودی", "/orders/inventory-control", ApplicationPermissions.OrdersViewInventory, 2),
        new(StableGuid.Create("menu:orders:needs"), DepartmentType.OrdersAndInventoryControl, "نیازهای کالا", "/orders/material-needs", ApplicationPermissions.OrdersCreateMaterialNeed, 3),
        new(StableGuid.Create("menu:orders:shortages"), DepartmentType.OrdersAndInventoryControl, "هشدارهای کمبود", "/orders/shortage-alerts", ApplicationPermissions.OrdersManageShortageAlerts, 4),
        new(StableGuid.Create("menu:orders:indents"), DepartmentType.OrdersAndInventoryControl, "درخواست‌های خرید", "/orders/indents", ApplicationPermissions.IndentView, 5),
        new(StableGuid.Create("menu:warehouse:tasks"), DepartmentType.Warehouse, "عملیات انبار", "/warehouse", ApplicationPermissions.WarehouseView, 1),
        new(StableGuid.Create("menu:warehouse:purchase-orders"), DepartmentType.Warehouse, "سفارش‌های در انتظار رسید", "/warehouse/purchase-orders", ApplicationPermissions.PurchaseOrderView, 2),
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
            PasswordHash = DefaultAdminPasswordHash,
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
                ApplicationPermissions.SupplierView,
                ApplicationPermissions.SupplierCreate,
                ApplicationPermissions.SupplierEdit,
                ApplicationPermissions.SupplierActivateDeactivate,
                ApplicationPermissions.SupplierBlacklist,
                ApplicationPermissions.SupplierManageContacts,
                ApplicationPermissions.SupplierManageCategories,
                ApplicationPermissions.SupplierEvaluate,
                ApplicationPermissions.SupplierManageDocuments,
                ApplicationPermissions.InquiryView,
                ApplicationPermissions.InquiryCreate,
                ApplicationPermissions.InquiryEdit,
                ApplicationPermissions.InquirySend,
                ApplicationPermissions.InquiryCancel,
                ApplicationPermissions.InquiryManageSuppliers,
                ApplicationPermissions.InquiryReceiveQuote,
                ApplicationPermissions.InquiryCompareQuotes,
                ApplicationPermissions.InquirySelectSupplier,
                ApplicationPermissions.InquiryManageDocuments,
                ApplicationPermissions.OrdersViewDashboard,
                ApplicationPermissions.OrdersViewInventory,
                ApplicationPermissions.TenderView,
                ApplicationPermissions.TenderCreate,
                ApplicationPermissions.TenderEdit,
                ApplicationPermissions.TenderPublish,
                ApplicationPermissions.TenderCancel,
                ApplicationPermissions.TenderManageItems,
                ApplicationPermissions.TenderManageParticipants,
                ApplicationPermissions.TenderReceiveBid,
                ApplicationPermissions.TenderCompareBids,
                ApplicationPermissions.TenderSelectWinner,
                ApplicationPermissions.TenderClose,
                ApplicationPermissions.TenderManageDocuments,
                ApplicationPermissions.TenderReportView,
                ApplicationPermissions.TenderReportExport,
                ApplicationPermissions.CommissionView,
                ApplicationPermissions.CommissionReportView,
                ApplicationPermissions.CommissionReportExport,
                ApplicationPermissions.ContractView,
                ApplicationPermissions.ContractCreate,
                ApplicationPermissions.ContractEdit,
                ApplicationPermissions.ContractSubmit,
                ApplicationPermissions.ContractApprove,
                ApplicationPermissions.ContractReject,
                ApplicationPermissions.ContractSign,
                ApplicationPermissions.ContractCancel,
                ApplicationPermissions.ContractManageClauses,
                ApplicationPermissions.ContractManageTemplates,
                ApplicationPermissions.ContractManageDocuments,
                ApplicationPermissions.ContractReportView,
                ApplicationPermissions.ContractReportExport,
                ApplicationPermissions.PurchaseOrderView,
                ApplicationPermissions.PurchaseOrderCreate,
                ApplicationPermissions.PurchaseOrderEdit,
                ApplicationPermissions.PurchaseOrderSubmit,
                ApplicationPermissions.PurchaseOrderApprove,
                ApplicationPermissions.PurchaseOrderReject,
                ApplicationPermissions.PurchaseOrderIssue,
                ApplicationPermissions.PurchaseOrderCancel,
                ApplicationPermissions.PurchaseOrderManageItems,
                ApplicationPermissions.PurchaseOrderManageDocuments,
                ApplicationPermissions.PurchaseOrderReportView,
                ApplicationPermissions.PurchaseOrderReportExport,
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
                ApplicationPermissions.ItemView,
                ApplicationPermissions.SupplierView,
                ApplicationPermissions.SupplierCreate,
                ApplicationPermissions.SupplierEdit,
                ApplicationPermissions.SupplierManageContacts,
                ApplicationPermissions.SupplierManageCategories,
                ApplicationPermissions.InquiryView,
                ApplicationPermissions.InquiryCreate,
                ApplicationPermissions.InquiryEdit,
                ApplicationPermissions.InquirySend,
                ApplicationPermissions.InquiryManageSuppliers,
                ApplicationPermissions.InquiryReceiveQuote,
                ApplicationPermissions.InquiryCompareQuotes,
                ApplicationPermissions.TenderView,
                ApplicationPermissions.TenderCreate,
                ApplicationPermissions.TenderEdit,
                ApplicationPermissions.TenderManageItems,
                ApplicationPermissions.TenderManageParticipants,
                ApplicationPermissions.TenderReceiveBid,
                ApplicationPermissions.TenderCompareBids,
                ApplicationPermissions.TenderManageDocuments,
                ApplicationPermissions.TenderReportView,
                ApplicationPermissions.TenderReportExport,
                ApplicationPermissions.CommissionView,
                ApplicationPermissions.ContractView,
                ApplicationPermissions.ContractCreate,
                ApplicationPermissions.ContractEdit,
                ApplicationPermissions.ContractSubmit,
                ApplicationPermissions.ContractManageClauses,
                ApplicationPermissions.ContractManageDocuments,
                ApplicationPermissions.ContractReportView,
                ApplicationPermissions.ContractReportExport,
                ApplicationPermissions.PurchaseOrderView,
                ApplicationPermissions.PurchaseOrderCreate,
                ApplicationPermissions.PurchaseOrderEdit,
                ApplicationPermissions.PurchaseOrderSubmit,
                ApplicationPermissions.PurchaseOrderManageItems,
                ApplicationPermissions.PurchaseOrderManageDocuments,
                ApplicationPermissions.PurchaseOrderReportView,
                ApplicationPermissions.PurchaseOrderReportExport
            ],
            [ApplicationRoles.OrdersManager] =
            [
                ApplicationPermissions.IndentCreate,
                ApplicationPermissions.IndentView,
                ApplicationPermissions.IndentApprove,
                ApplicationPermissions.IndentSendToPurchase,
                ApplicationPermissions.ItemView,
                ApplicationPermissions.OrdersViewDashboard,
                ApplicationPermissions.OrdersViewInventory,
                ApplicationPermissions.OrdersManageInventoryControl,
                ApplicationPermissions.OrdersCreateMaterialNeed,
                ApplicationPermissions.OrdersReviewMaterialNeed,
                ApplicationPermissions.OrdersApproveMaterialNeed,
                ApplicationPermissions.OrdersConvertNeedToIndent,
                ApplicationPermissions.OrdersConvertShortageToIndent,
                ApplicationPermissions.OrdersManageShortageAlerts
            ],
            [ApplicationRoles.OrdersUser] =
            [
                ApplicationPermissions.IndentCreate,
                ApplicationPermissions.IndentView,
                ApplicationPermissions.IndentSendToPurchase,
                ApplicationPermissions.ItemView,
                ApplicationPermissions.OrdersViewDashboard,
                ApplicationPermissions.OrdersViewInventory,
                ApplicationPermissions.OrdersCreateMaterialNeed,
                ApplicationPermissions.OrdersReviewMaterialNeed,
                ApplicationPermissions.OrdersConvertNeedToIndent,
                ApplicationPermissions.OrdersManageShortageAlerts
            ],
            [ApplicationRoles.WarehouseManager] =
            [
                ApplicationPermissions.WarehouseView,
                ApplicationPermissions.WarehouseReceive,
                ApplicationPermissions.WarehouseIssue,
                ApplicationPermissions.WarehouseManageWarehouses,
                ApplicationPermissions.WarehouseReceiptView,
                ApplicationPermissions.WarehouseReceiptCreate,
                ApplicationPermissions.WarehouseReceiptEdit,
                ApplicationPermissions.WarehouseReceiptSubmit,
                ApplicationPermissions.WarehouseReceiptApprove,
                ApplicationPermissions.WarehouseReceiptCancel,
                ApplicationPermissions.WarehouseReceiptManageDocuments,
                ApplicationPermissions.WarehouseReceiptReportView,
                ApplicationPermissions.WarehouseReceiptReportExport,
                ApplicationPermissions.InventoryViewTransactions,
                ApplicationPermissions.InventoryViewStockBalance,
                ApplicationPermissions.PurchaseOrderView,
                ApplicationPermissions.ItemView
            ],
            [ApplicationRoles.WarehouseUser] =
            [
                ApplicationPermissions.WarehouseView,
                ApplicationPermissions.WarehouseReceive,
                ApplicationPermissions.WarehouseReceiptView,
                ApplicationPermissions.WarehouseReceiptCreate,
                ApplicationPermissions.WarehouseReceiptEdit,
                ApplicationPermissions.WarehouseReceiptSubmit,
                ApplicationPermissions.WarehouseReceiptManageDocuments,
                ApplicationPermissions.InventoryViewTransactions,
                ApplicationPermissions.InventoryViewStockBalance,
                ApplicationPermissions.PurchaseOrderView,
                ApplicationPermissions.ItemView
            ],
            [ApplicationRoles.ApplicantUser] =
            [
                ApplicationPermissions.IndentCreate,
                ApplicationPermissions.IndentView,
                ApplicationPermissions.PurchaseFileView,
                ApplicationPermissions.ItemView,
                ApplicationPermissions.OrdersCreateMaterialNeed
            ],
            [ApplicationRoles.TenderCommissionManager] =
            [
                ApplicationPermissions.TenderCreate,
                ApplicationPermissions.TenderView,
                ApplicationPermissions.TenderEvaluateQualification,
                ApplicationPermissions.TenderEvaluateTechnical,
                ApplicationPermissions.TenderEvaluateCommercial,
                ApplicationPermissions.TenderCompareBids,
                ApplicationPermissions.TenderSelectWinner,
                ApplicationPermissions.TenderEvaluate,
                ApplicationPermissions.TenderApproveWinner,
                ApplicationPermissions.TenderReportView,
                ApplicationPermissions.TenderReportExport,
                ApplicationPermissions.CommissionView,
                ApplicationPermissions.CommissionCreate,
                ApplicationPermissions.CommissionEdit,
                ApplicationPermissions.CommissionSchedule,
                ApplicationPermissions.CommissionStart,
                ApplicationPermissions.CommissionComplete,
                ApplicationPermissions.CommissionApprove,
                ApplicationPermissions.CommissionCancel,
                ApplicationPermissions.CommissionManageMembers,
                ApplicationPermissions.CommissionManageAgenda,
                ApplicationPermissions.CommissionManageMinutes,
                ApplicationPermissions.CommissionManageDecisions,
                ApplicationPermissions.CommissionManageDocuments,
                ApplicationPermissions.CommissionReportView,
                ApplicationPermissions.CommissionReportExport,
                ApplicationPermissions.PurchaseFileView,
                ApplicationPermissions.ContractView,
                ApplicationPermissions.SupplierView,
                ApplicationPermissions.SupplierEvaluate,
                ApplicationPermissions.InquiryView,
                ApplicationPermissions.InquiryCompareQuotes
            ],
            [ApplicationRoles.TenderCommissionMember] =
            [
                ApplicationPermissions.TenderView,
                ApplicationPermissions.TenderEvaluateQualification,
                ApplicationPermissions.TenderEvaluateTechnical,
                ApplicationPermissions.TenderEvaluateCommercial,
                ApplicationPermissions.TenderCompareBids,
                ApplicationPermissions.TenderEvaluate,
                ApplicationPermissions.TenderReportView,
                ApplicationPermissions.CommissionView,
                ApplicationPermissions.CommissionManageMinutes,
                ApplicationPermissions.CommissionManageDecisions,
                ApplicationPermissions.CommissionReportView,
                ApplicationPermissions.PurchaseFileView
            ],
            [ApplicationRoles.ReportViewer] =
            [
                ApplicationPermissions.ReportView,
                ApplicationPermissions.ReportPrint,
                ApplicationPermissions.ReportExportPdf,
                ApplicationPermissions.TenderReportView,
                ApplicationPermissions.TenderReportExport,
                ApplicationPermissions.CommissionReportView,
                ApplicationPermissions.CommissionReportExport,
                ApplicationPermissions.ContractReportView,
                ApplicationPermissions.ContractReportExport,
                ApplicationPermissions.PurchaseOrderReportView,
                ApplicationPermissions.PurchaseOrderReportExport,
                ApplicationPermissions.WarehouseReceiptReportView,
                ApplicationPermissions.WarehouseReceiptReportExport
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
