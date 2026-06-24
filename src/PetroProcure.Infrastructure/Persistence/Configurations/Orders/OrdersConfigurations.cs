using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Orders;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Orders;

internal sealed class InventoryControlItemConfiguration : IEntityTypeConfiguration<InventoryControlItem>
{
    public void Configure(EntityTypeBuilder<InventoryControlItem> builder)
    {
        builder.ToTable("InventoryControlItems", DatabaseSchemas.Orders);
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.MescItemId).IsUnique();
        builder.Property(x => x.MescCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.MescGeneralGroupCode).HasMaxLength(6).IsRequired();
        builder.Property(x => x.GeneralDescription).HasMaxLength(512).IsRequired();
        builder.Property(x => x.SpecificDescription).HasMaxLength(512).IsRequired();
        builder.Property(x => x.MinimumStockLevel).HasPrecision(18, 4);
        builder.Property(x => x.ReorderPoint).HasPrecision(18, 4);
        builder.Property(x => x.MaximumStockLevel).HasPrecision(18, 4);
        builder.Property(x => x.SafetyStock).HasPrecision(18, 4);
        builder.Property(x => x.Notes).HasMaxLength(1024);
    }
}

internal sealed class StockBalanceConfiguration : IEntityTypeConfiguration<StockBalance>
{
    public void Configure(EntityTypeBuilder<StockBalance> builder)
    {
        builder.ToTable("StockBalances", DatabaseSchemas.Orders);
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.MescItemId, x.WarehouseId }).IsUnique().HasFilter(null);
        builder.Property(x => x.AvailableQuantity).HasPrecision(18, 4);
        builder.Property(x => x.ReservedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.OnOrderQuantity).HasPrecision(18, 4);
    }
}

internal sealed class MaterialNeedConfiguration : IEntityTypeConfiguration<MaterialNeed>
{
    public void Configure(EntityTypeBuilder<MaterialNeed> builder)
    {
        builder.ToTable("MaterialNeeds", DatabaseSchemas.Orders);
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.NeedNumber).IsUnique();
        builder.Property(x => x.NeedNumber).HasMaxLength(32).IsRequired();
        builder.Property(x => x.MescCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.MescGeneralGroupCode).HasMaxLength(6).IsRequired();
        builder.Property(x => x.GeneralDescription).HasMaxLength(512).IsRequired();
        builder.Property(x => x.SpecificDescription).HasMaxLength(512).IsRequired();
        builder.Property(x => x.RequestedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.Description).HasMaxLength(2048);
        builder.Property(x => x.RejectionReason).HasMaxLength(2048);
    }
}

internal sealed class ShortageAlertConfiguration : IEntityTypeConfiguration<ShortageAlert>
{
    public void Configure(EntityTypeBuilder<ShortageAlert> builder)
    {
        builder.ToTable("ShortageAlerts", DatabaseSchemas.Orders);
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.MescItemId, x.Status });
        builder.Property(x => x.MescCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.MescGeneralGroupCode).HasMaxLength(6).IsRequired();
        builder.Property(x => x.GeneralDescription).HasMaxLength(512).IsRequired();
        builder.Property(x => x.SpecificDescription).HasMaxLength(512).IsRequired();
        builder.Property(x => x.CurrentStock).HasPrecision(18, 4);
        builder.Property(x => x.ReorderPoint).HasPrecision(18, 4);
        builder.Property(x => x.ShortageQuantity).HasPrecision(18, 4);
        builder.Property(x => x.ResolutionNote).HasMaxLength(2048);
    }
}

internal sealed class MaterialNeedSequenceConfiguration : IEntityTypeConfiguration<MaterialNeedSequence>
{
    public void Configure(EntityTypeBuilder<MaterialNeedSequence> builder)
    {
        builder.ToTable("MaterialNeedSequences", DatabaseSchemas.Orders);
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Year).IsUnique();
    }
}
