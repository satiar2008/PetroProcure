using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.Items;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Infrastructure.Persistence.Configurations;

namespace PetroProcure.Infrastructure.Persistence.Configurations.PurchaseFiles;

internal sealed class PurchaseFileItemConfiguration : IEntityTypeConfiguration<PurchaseFileItem>
{
    public void Configure(EntityTypeBuilder<PurchaseFileItem> builder)
    {
        builder.ToTable("PurchaseFileItems", DatabaseSchemas.Purchase);
        builder.ConfigureAuditableEntity();
        builder.Property(item => item.MescCode).IsRequired().HasMaxLength(50);
        builder.Property(item => item.MescGeneralGroupCode).IsRequired().HasMaxLength(6);
        builder.Property(item => item.GeneralDescription).IsRequired().HasMaxLength(500);
        builder.Property(item => item.SpecificDescription).IsRequired().HasMaxLength(500);
        builder.Property(item => item.RequestedQuantity).IsRequired().HasPrecision(18, 4);
        builder.Property(item => item.ApprovedQuantity).IsRequired().HasPrecision(18, 4);
        builder.Property(item => item.TechnicalDescription).HasMaxLength(2000);
        builder.HasIndex(item => item.PurchaseFileId);
        builder.HasIndex(item => item.MescGeneralGroupCode);
        builder.HasOne<MescItem>().WithMany().HasForeignKey(item => item.MescItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<UnitOfMeasure>().WithMany().HasForeignKey(item => item.UnitOfMeasureId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<IndentItem>().WithMany().HasForeignKey(item => item.SourceIndentItemId).OnDelete(DeleteBehavior.Restrict);
    }
}
