using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Contracts;
using PetroProcure.Domain.Modules.Documents;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.Suppliers;
using PetroProcure.Domain.Modules.TenderCommission;
using PetroProcure.Domain.Modules.Tenders;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Infrastructure.Persistence.Seeding;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Contracts;

internal sealed class PurchaseContractConfiguration : IEntityTypeConfiguration<PurchaseContract>
{
    public void Configure(EntityTypeBuilder<PurchaseContract> b)
    {
        b.ToTable("PurchaseContracts", DatabaseSchemas.Contract);
        b.ConfigureEntity();
        b.Property(x => x.ContractNumber).HasMaxLength(20).IsRequired();
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.Subject).HasMaxLength(1000).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.ContractType).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.TaxAmount).HasPrecision(18, 2);
        b.Property(x => x.FinalAmount).HasPrecision(18, 2);
        b.Property(x => x.PaymentTerms).HasMaxLength(2000);
        b.Property(x => x.DeliveryTerms).HasMaxLength(2000);
        b.Property(x => x.WarrantyTerms).HasMaxLength(2000);
        b.Property(x => x.PenaltyTerms).HasMaxLength(2000);
        b.Property(x => x.Description).HasMaxLength(4000);
        b.Property(x => x.CancellationReason).HasMaxLength(2000);
        b.HasIndex(x => x.ContractNumber).IsUnique();
        b.HasIndex(x => x.PurchaseFileId);
        b.HasIndex(x => x.SupplierId);
        b.HasIndex(x => x.TenderId);
        b.HasIndex(x => x.TenderBidId);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.CreatedAt);
        b.HasIndex(x => x.StartDate);
        b.HasIndex(x => x.EndDate);
        b.HasOne<PurchaseFile>().WithMany().HasForeignKey(x => x.PurchaseFileId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Supplier>().WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Tender>().WithMany().HasForeignKey(x => x.TenderId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<TenderBid>().WithMany().HasForeignKey(x => x.TenderBidId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<TenderCommissionDecision>().WithMany().HasForeignKey(x => x.CommissionDecisionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ContractTemplate>().WithMany().HasForeignKey(x => x.ContractTemplateId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Clauses).WithOne().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Approvals).WithOne().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Documents).WithOne().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.Clauses).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.Approvals).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.Documents).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class PurchaseContractItemConfiguration : IEntityTypeConfiguration<PurchaseContractItem>
{
    public void Configure(EntityTypeBuilder<PurchaseContractItem> b)
    {
        b.ToTable("PurchaseContractItems", DatabaseSchemas.Contract);
        b.ConfigureEntity();
        b.Property(x => x.MescCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.MescGeneralGroupCode).HasMaxLength(6).IsRequired();
        b.Property(x => x.GeneralDescription).HasMaxLength(500).IsRequired();
        b.Property(x => x.SpecificDescription).HasMaxLength(1000).IsRequired();
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.TotalPrice).HasPrecision(18, 2);
        b.Property(x => x.TechnicalDescription).HasMaxLength(2000);
        b.HasIndex(x => x.ContractId);
        b.HasIndex(x => x.MescGeneralGroupCode);
        b.HasOne<PurchaseFileItem>().WithMany().HasForeignKey(x => x.PurchaseFileItemId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<TenderBidItem>().WithMany().HasForeignKey(x => x.TenderBidItemId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ContractClauseConfiguration : IEntityTypeConfiguration<ContractClause>
{
    public void Configure(EntityTypeBuilder<ContractClause> b)
    {
        b.ToTable("ContractClauses", DatabaseSchemas.Contract);
        b.ConfigureEntity();
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.Body).HasMaxLength(8000).IsRequired();
        b.Property(x => x.ClauseType).HasConversion<string>().HasMaxLength(50);
        b.HasIndex(x => new { x.ContractId, x.OrderNo });
    }
}

internal sealed class ContractTemplateConfiguration : IEntityTypeConfiguration<ContractTemplate>
{
    public void Configure(EntityTypeBuilder<ContractTemplate> b)
    {
        b.ToTable("ContractTemplates", DatabaseSchemas.Contract);
        b.ConfigureEntity();
        b.Property(x => x.TemplateCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.ContractType).HasConversion<string>().HasMaxLength(50);
        b.HasIndex(x => x.TemplateCode).IsUnique();
        b.HasMany(x => x.Clauses).WithOne().HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Clauses).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasData(ContractSeed.Templates);
    }
}

internal sealed class ContractTemplateClauseConfiguration : IEntityTypeConfiguration<ContractTemplateClause>
{
    public void Configure(EntityTypeBuilder<ContractTemplateClause> b)
    {
        b.ToTable("ContractTemplateClauses", DatabaseSchemas.Contract);
        b.ConfigureEntity();
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.Body).HasMaxLength(8000).IsRequired();
        b.Property(x => x.ClauseType).HasConversion<string>().HasMaxLength(50);
        b.HasIndex(x => new { x.TemplateId, x.OrderNo });
        b.HasData(ContractSeed.TemplateClauses);
    }
}

internal sealed class ContractApprovalConfiguration : IEntityTypeConfiguration<ContractApproval>
{
    public void Configure(EntityTypeBuilder<ContractApproval> b)
    {
        b.ToTable("ContractApprovals", DatabaseSchemas.Contract);
        b.ConfigureEntity();
        b.Property(x => x.ApprovalStep).HasMaxLength(100).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.Comment).HasMaxLength(2000);
        b.HasIndex(x => new { x.ContractId, x.Status });
    }
}

internal sealed class ContractDocumentConfiguration : IEntityTypeConfiguration<ContractDocument>
{
    public void Configure(EntityTypeBuilder<ContractDocument> b)
    {
        b.ToTable("ContractDocuments", DatabaseSchemas.Contract);
        b.ConfigureEntity();
        b.Property(x => x.DocumentType).HasMaxLength(100).IsRequired();
        b.Property(x => x.OriginalFileName).HasMaxLength(260);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.HasIndex(x => x.ContractId);
        b.HasOne<FileDocument>().WithMany().HasForeignKey(x => x.FileDocumentId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class ContractSequenceConfiguration : IEntityTypeConfiguration<ContractSequence>
{
    public void Configure(EntityTypeBuilder<ContractSequence> b)
    {
        b.ToTable("ContractSequences", DatabaseSchemas.Contract);
        b.ConfigureEntity();
        b.HasIndex(x => x.Year).IsUnique();
    }
}

internal static class ContractSeed
{
    public static readonly Guid DirectPurchaseTemplateId = Guid.Parse("d1000000-0000-0000-0000-000000000001");
    public static readonly Guid TenderTemplateId = Guid.Parse("d1000000-0000-0000-0000-000000000002");
    private static readonly DateTime CreatedAt = new(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

    public static readonly object[] Templates =
    [
        new
        {
            Id = DirectPurchaseTemplateId,
            TemplateCode = "DIRECT-BASE",
            Title = "قالب پایه قرارداد خرید مستقیم",
            Description = "قالب عمومی برای قراردادهای خرید مستقیم",
            ContractType = ContractType.DirectPurchase,
            IsActive = true,
            CreatedAt,
            CreatedByUserId = IdentitySeedData.DefaultAdminUserId
        },
        new
        {
            Id = TenderTemplateId,
            TemplateCode = "TENDER-BASE",
            Title = "قالب پایه قرارداد مناقصه",
            Description = "قالب عمومی برای قراردادهای مبتنی بر مناقصه",
            ContractType = ContractType.TenderBased,
            IsActive = true,
            CreatedAt,
            CreatedByUserId = IdentitySeedData.DefaultAdminUserId
        }
    ];

    public static readonly object[] TemplateClauses =
    [
        Clause("d2000000-0000-0000-0000-000000000001", DirectPurchaseTemplateId, 1, "موضوع قرارداد", "موضوع قرارداد عبارت است از تأمین کالا/خدمات مندرج در پیوست فنی.", ContractClauseType.General, true, true),
        Clause("d2000000-0000-0000-0000-000000000002", DirectPurchaseTemplateId, 2, "شرایط پرداخت", "پرداخت طبق تأیید واحد خرید و پس از تحویل/ارائه مدارک معتبر انجام می‌شود.", ContractClauseType.Payment, true, true),
        Clause("d2000000-0000-0000-0000-000000000003", DirectPurchaseTemplateId, 3, "تحویل", "تأمین‌کننده موظف است اقلام را طبق زمان‌بندی توافق‌شده تحویل دهد.", ContractClauseType.Delivery, true, true),
        Clause("d2000000-0000-0000-0000-000000000004", TenderTemplateId, 1, "مبنای قرارداد", "این قرارداد براساس مناقصه و تصمیم/مصوبه کمیسیون مربوطه تنظیم شده است.", ContractClauseType.Legal, true, true),
        Clause("d2000000-0000-0000-0000-000000000005", TenderTemplateId, 2, "مشخصات فنی", "مشخصات فنی و مقادیر مطابق اسناد مناقصه و پیشنهاد منتخب ملاک عمل است.", ContractClauseType.Technical, true, true),
        Clause("d2000000-0000-0000-0000-000000000006", TenderTemplateId, 3, "ضمانت و جریمه", "ضمانت‌ها و جرائم تأخیر مطابق شرایط اختصاصی قرارداد اعمال می‌شود.", ContractClauseType.Penalty, true, true)
    ];

    private static object Clause(string id, Guid templateId, int orderNo, string title, string body,
        ContractClauseType type, bool required, bool editable) => new
    {
        Id = Guid.Parse(id),
        TemplateId = templateId,
        OrderNo = orderNo,
        Title = title,
        Body = body,
        ClauseType = type,
        IsRequired = required,
        IsEditable = editable
    };
}
