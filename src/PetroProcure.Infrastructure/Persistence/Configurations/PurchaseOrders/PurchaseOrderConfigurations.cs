using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Contracts;
using PetroProcure.Domain.Modules.Documents;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.PurchaseOrders;
using PetroProcure.Domain.Modules.Suppliers;
using PetroProcure.Domain.Modules.Tenders;
using PetroProcure.Infrastructure.Identity;

namespace PetroProcure.Infrastructure.Persistence.Configurations.PurchaseOrders;

internal sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> b)
    {
        b.ToTable("PurchaseOrders", DatabaseSchemas.PurchaseOrder);
        b.ConfigureEntity();
        b.Property(x => x.PurchaseOrderNumber).HasMaxLength(20).IsRequired();
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.Description).HasMaxLength(4000);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.PurchaseOrderType).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.TaxAmount).HasPrecision(18, 2);
        b.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        b.Property(x => x.FinalAmount).HasPrecision(18, 2);
        b.Property(x => x.DeliveryLocation).HasMaxLength(500);
        b.Property(x => x.DeliveryTerms).HasMaxLength(2000);
        b.Property(x => x.PaymentTerms).HasMaxLength(2000);
        b.Property(x => x.WarrantyTerms).HasMaxLength(2000);
        b.Property(x => x.Notes).HasMaxLength(4000);
        b.Property(x => x.CancellationReason).HasMaxLength(2000);
        b.HasIndex(x => x.PurchaseOrderNumber).IsUnique();
        b.HasIndex(x => x.PurchaseFileId);
        b.HasIndex(x => x.SupplierId);
        b.HasIndex(x => x.ContractId);
        b.HasIndex(x => x.TenderId);
        b.HasIndex(x => x.TenderBidId);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.CreatedAt);
        b.HasIndex(x => x.OrderDate);
        b.HasIndex(x => x.ExpectedDeliveryDate);
        b.HasOne<PurchaseFile>().WithMany().HasForeignKey(x => x.PurchaseFileId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Supplier>().WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<PurchaseContract>().WithMany().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Tender>().WithMany().HasForeignKey(x => x.TenderId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<TenderBid>().WithMany().HasForeignKey(x => x.TenderBidId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Approvals).WithOne().HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Documents).WithOne().HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.Approvals).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.Documents).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> b)
    {
        b.ToTable("PurchaseOrderItems", DatabaseSchemas.PurchaseOrder);
        b.ConfigureEntity();
        b.Property(x => x.MescCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.MescGeneralGroupCode).HasMaxLength(6).IsRequired();
        b.Property(x => x.GeneralDescription).HasMaxLength(500).IsRequired();
        b.Property(x => x.SpecificDescription).HasMaxLength(1000).IsRequired();
        b.Property(x => x.OrderedQuantity).HasPrecision(18, 4);
        b.Property(x => x.ReceivedQuantity).HasPrecision(18, 4);
        b.Property(x => x.RemainingQuantity).HasPrecision(18, 4);
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.TotalPrice).HasPrecision(18, 2);
        b.Property(x => x.TechnicalDescription).HasMaxLength(2000);
        b.Property(x => x.Notes).HasMaxLength(2000);
        b.HasIndex(x => x.PurchaseOrderId);
        b.HasIndex(x => x.MescGeneralGroupCode);
        b.HasOne<PurchaseFileItem>().WithMany().HasForeignKey(x => x.PurchaseFileItemId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<PurchaseContractItem>().WithMany().HasForeignKey(x => x.ContractItemId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<TenderBidItem>().WithMany().HasForeignKey(x => x.TenderBidItemId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PurchaseOrderApprovalConfiguration : IEntityTypeConfiguration<PurchaseOrderApproval>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderApproval> b)
    {
        b.ToTable("PurchaseOrderApprovals", DatabaseSchemas.PurchaseOrder);
        b.ConfigureEntity();
        b.Property(x => x.ApprovalStep).HasMaxLength(100).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.Comment).HasMaxLength(2000);
        b.HasIndex(x => new { x.PurchaseOrderId, x.Status });
    }
}

internal sealed class PurchaseOrderDocumentConfiguration : IEntityTypeConfiguration<PurchaseOrderDocument>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderDocument> b)
    {
        b.ToTable("PurchaseOrderDocuments", DatabaseSchemas.PurchaseOrder);
        b.ConfigureEntity();
        b.Property(x => x.DocumentType).HasMaxLength(100).IsRequired();
        b.Property(x => x.OriginalFileName).HasMaxLength(260);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.HasIndex(x => x.PurchaseOrderId);
        b.HasOne<FileDocument>().WithMany().HasForeignKey(x => x.FileDocumentId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class PurchaseOrderSequenceConfiguration : IEntityTypeConfiguration<PurchaseOrderSequence>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderSequence> b)
    {
        b.ToTable("PurchaseOrderSequences", DatabaseSchemas.PurchaseOrder);
        b.ConfigureEntity();
        b.HasIndex(x => x.Year).IsUnique();
    }
}
