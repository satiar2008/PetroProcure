using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.Items;
using PetroProcure.Infrastructure.Persistence.Configurations;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Indents;

internal sealed class IndentItemConfiguration : IEntityTypeConfiguration<IndentItem>
{
    public void Configure(EntityTypeBuilder<IndentItem> builder)
    {
        builder.ToTable("IndentItems", DatabaseSchemas.Indent);
        builder.ConfigureAuditableEntity();
        builder.Property(item => item.MescCode).IsRequired().HasMaxLength(50);
        builder.Property(item => item.MescGeneralGroupCode).IsRequired().HasMaxLength(6).IsFixedLength();
        builder.Property(item => item.GeneralDescription).IsRequired().HasMaxLength(500);
        builder.Property(item => item.SpecificDescription).IsRequired().HasMaxLength(500);
        builder.Property(item => item.RequestedQuantity).IsRequired().HasPrecision(18, 4);
        builder.Property(item => item.TechnicalDescription).HasMaxLength(2000);
        builder.Property(item => item.RequiredDate).HasColumnType("date");
        builder.HasIndex(item => item.IndentId);
        builder.HasIndex(item => item.MescGeneralGroupCode);
        builder.HasOne<MescItem>().WithMany().HasForeignKey(item => item.MescItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<UnitOfMeasure>().WithMany().HasForeignKey(item => item.UnitOfMeasureId).OnDelete(DeleteBehavior.Restrict);
    }
}
