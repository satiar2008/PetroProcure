using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Application.Security;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Workflow;
using PetroProcure.Infrastructure.Persistence.Configurations;
using PetroProcure.Infrastructure.Persistence.Seeding;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Workflow;

internal sealed class WorkflowActionDefinitionConfiguration : IEntityTypeConfiguration<WorkflowActionDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowActionDefinition> b)
    {
        b.ToTable("WorkflowActionDefinitions", DatabaseSchemas.Workflow);
        b.ConfigureEntity();
        b.Property(x => x.Code).IsRequired().HasMaxLength(100);
        b.Property(x => x.Title).IsRequired().HasMaxLength(300);
        b.Property(x => x.RequiredPermission).IsRequired().HasMaxLength(200);
        b.HasIndex(x => x.Code).IsUnique();
        b.HasIndex(x => new { x.FromDepartmentType, x.FromStatus, x.IsActive });
        b.HasData(Definitions());
    }

    internal static WorkflowActionDefinition[] Definitions() =>
    [
        Action("ORDERS_TO_PURCHASE", "ارسال به واحد خرید", DepartmentType.OrdersAndInventoryControl,
            DepartmentType.PurchaseDepartment, PurchaseFileStatus.WaitingForPurchaseDepartment, PurchaseFileStatus.InPurchaseDepartment),
        Action("PURCHASE_TO_APPLICANT", "ارسال برای بررسی فنی", DepartmentType.PurchaseDepartment,
            DepartmentType.Applicant, PurchaseFileStatus.InPurchaseDepartment, PurchaseFileStatus.WaitingForTechnicalReview, true),
        Action("APPLICANT_TO_PURCHASE", "اتمام بررسی فنی و بازگشت به خرید", DepartmentType.Applicant,
            DepartmentType.PurchaseDepartment, PurchaseFileStatus.WaitingForTechnicalReview, PurchaseFileStatus.InPurchaseDepartment, true),
        Action("PURCHASE_TO_TENDER", "ارسال به کمیسیون مناقصه", DepartmentType.PurchaseDepartment,
            DepartmentType.TenderCommission, PurchaseFileStatus.InPurchaseDepartment, PurchaseFileStatus.WaitingForTenderCommission, true),
        Action("TENDER_TO_PURCHASE", "بازگشت نتیجه کمیسیون به خرید", DepartmentType.TenderCommission,
            DepartmentType.PurchaseDepartment, PurchaseFileStatus.WaitingForTenderCommission, PurchaseFileStatus.InPurchaseDepartment, true),
        Action("PURCHASE_TO_WAREHOUSE", "ارسال به انبار", DepartmentType.PurchaseDepartment,
            DepartmentType.Warehouse, PurchaseFileStatus.InPurchaseDepartment, PurchaseFileStatus.WaitingForWarehouseReceipt),
        Action("WAREHOUSE_TO_PURCHASE", "بازگشت رسید انبار به خرید", DepartmentType.Warehouse,
            DepartmentType.PurchaseDepartment, PurchaseFileStatus.WaitingForWarehouseReceipt, PurchaseFileStatus.InPurchaseDepartment, true),
        Action("PURCHASE_COMPLETE", "تکمیل پرونده", DepartmentType.PurchaseDepartment,
            null, PurchaseFileStatus.InPurchaseDepartment, PurchaseFileStatus.Completed, true, false, true),
        Action("RETURN_PREVIOUS", "بازگشت به واحد قبلی", DepartmentType.PurchaseDepartment,
            null, PurchaseFileStatus.InPurchaseDepartment, PurchaseFileStatus.WaitingForTechnicalReview, true, true)
    ];

    private static WorkflowActionDefinition Action(
        string code, string title, DepartmentType fromDepartment, DepartmentType? toDepartment,
        PurchaseFileStatus fromStatus, PurchaseFileStatus toStatus, bool comment = false,
        bool isReturn = false, bool final = false) =>
        new(StableGuid.Create($"workflow-action:{code}"), code, title, fromDepartment, toDepartment,
            fromStatus, toStatus, ApplicationPermissions.PurchaseFileSendToDepartment,
            comment, isReturn, final);
}
