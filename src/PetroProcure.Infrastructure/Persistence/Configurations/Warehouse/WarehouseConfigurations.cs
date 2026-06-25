using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Documents;
using PetroProcure.Domain.Modules.Items;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.PurchaseOrders;
using PetroProcure.Domain.Modules.Suppliers;
using PetroProcure.Domain.Modules.Warehouse;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Warehouse;

internal sealed class WarehouseConfiguration : IEntityTypeConfiguration<Domain.Modules.Warehouse.Warehouse>
{
    public static readonly Guid MainWarehouseId = Guid.Parse("eeee0000-0000-0000-0000-000000000001");
    public void Configure(EntityTypeBuilder<Domain.Modules.Warehouse.Warehouse> b)
    {
        b.ToTable("Warehouses", DatabaseSchemas.Warehouse);
        b.ConfigureEntity();
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Location).HasMaxLength(500);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.HasIndex(x => x.Code).IsUnique();
        b.HasData(new { Id = MainWarehouseId, Code = "MAIN", Name = "انبار مرکزی", Location = "انبار مرکزی", IsActive = true, Description = "انبار پیش‌فرض سیستم" });
    }
}

internal sealed class WarehouseReceiptConfiguration : IEntityTypeConfiguration<WarehouseReceipt>
{
    public void Configure(EntityTypeBuilder<WarehouseReceipt> b)
    {
        b.ToTable("WarehouseReceipts", DatabaseSchemas.Warehouse);
        b.ConfigureEntity();
        b.Property(x => x.ReceiptNumber).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.DeliveryNoteNumber).HasMaxLength(100);
        b.Property(x => x.CarrierName).HasMaxLength(200);
        b.Property(x => x.VehicleNumber).HasMaxLength(100);
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.CancellationReason).HasMaxLength(2000);
        b.HasIndex(x => x.ReceiptNumber).IsUnique();
        b.HasIndex(x => x.PurchaseOrderId);
        b.HasIndex(x => x.PurchaseFileId);
        b.HasIndex(x => x.SupplierId);
        b.HasIndex(x => x.WarehouseId);
        b.HasIndex(x => x.ReceiptDate);
        b.HasIndex(x => x.Status);
        b.HasOne<PurchaseOrder>().WithMany().HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<PurchaseFile>().WithMany().HasForeignKey(x => x.PurchaseFileId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Supplier>().WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Domain.Modules.Warehouse.Warehouse>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.WarehouseReceiptId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Documents).WithOne().HasForeignKey(x => x.WarehouseReceiptId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.Documents).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class WarehouseReceiptItemConfiguration : IEntityTypeConfiguration<WarehouseReceiptItem>
{
    public void Configure(EntityTypeBuilder<WarehouseReceiptItem> b)
    {
        b.ToTable("WarehouseReceiptItems", DatabaseSchemas.Warehouse);
        b.ConfigureEntity();
        b.Property(x => x.MescCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.MescGeneralGroupCode).HasMaxLength(6).IsRequired();
        b.Property(x => x.GeneralDescription).HasMaxLength(500).IsRequired();
        b.Property(x => x.SpecificDescription).HasMaxLength(1000).IsRequired();
        b.Property(x => x.OrderedQuantity).HasPrecision(18, 4);
        b.Property(x => x.PreviouslyReceivedQuantity).HasPrecision(18, 4);
        b.Property(x => x.ReceivedQuantity).HasPrecision(18, 4);
        b.Property(x => x.AcceptedQuantity).HasPrecision(18, 4);
        b.Property(x => x.RejectedQuantity).HasPrecision(18, 4);
        b.Property(x => x.RemainingQuantityAfterReceipt).HasPrecision(18, 4);
        b.Property(x => x.QualityStatus).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.BatchNumber).HasMaxLength(100);
        b.Property(x => x.SerialNumber).HasMaxLength(100);
        b.Property(x => x.Notes).HasMaxLength(2000);
        b.HasIndex(x => x.MescItemId);
        b.HasOne<PurchaseOrderItem>().WithMany().HasForeignKey(x => x.PurchaseOrderItemId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class WarehouseReceiptDocumentConfiguration : IEntityTypeConfiguration<WarehouseReceiptDocument>
{
    public void Configure(EntityTypeBuilder<WarehouseReceiptDocument> b)
    {
        b.ToTable("WarehouseReceiptDocuments", DatabaseSchemas.Warehouse);
        b.ConfigureEntity();
        b.Property(x => x.DocumentType).HasMaxLength(100).IsRequired();
        b.Property(x => x.OriginalFileName).HasMaxLength(260);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.HasOne<FileDocument>().WithMany().HasForeignKey(x => x.FileDocumentId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> b)
    {
        b.ToTable("InventoryTransactions", DatabaseSchemas.Warehouse);
        b.ConfigureEntity();
        b.Property(x => x.TransactionNumber).HasMaxLength(24).IsRequired();
        b.Property(x => x.TransactionType).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.ReferenceType).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.Property(x => x.Description).HasMaxLength(2000);
        b.HasIndex(x => x.TransactionNumber).IsUnique();
        b.HasIndex(x => new { x.ReferenceType, x.ReferenceId });
        b.HasOne<MescItem>().WithMany().HasForeignKey(x => x.MescItemId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Domain.Modules.Warehouse.Warehouse>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class WarehouseReceiptSequenceConfiguration : IEntityTypeConfiguration<WarehouseReceiptSequence>
{
    public void Configure(EntityTypeBuilder<WarehouseReceiptSequence> b) { b.ToTable("WarehouseReceiptSequences", DatabaseSchemas.Warehouse); b.ConfigureEntity(); b.HasIndex(x => x.Year).IsUnique(); }
}
internal sealed class InventoryTransactionSequenceConfiguration : IEntityTypeConfiguration<InventoryTransactionSequence>
{
    public void Configure(EntityTypeBuilder<InventoryTransactionSequence> b) { b.ToTable("InventoryTransactionSequences", DatabaseSchemas.Warehouse); b.ConfigureEntity(); b.HasIndex(x => x.Year).IsUnique(); }
}
