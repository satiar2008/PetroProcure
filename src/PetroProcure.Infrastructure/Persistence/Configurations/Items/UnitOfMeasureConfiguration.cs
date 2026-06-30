using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Items;
using PetroProcure.Infrastructure.Persistence.Configurations;
using PetroProcure.Infrastructure.Persistence.Seeding;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Items;

internal sealed class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("UnitOfMeasures", DatabaseSchemas.Item);
        builder.ConfigureAuditableEntity();

        builder.Property(unit => unit.Code)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(unit => unit.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(unit => unit.IsActive)
            .IsRequired();

        builder.HasIndex(unit => unit.Code).IsUnique();
        builder.HasIndex(unit => unit.Name).IsUnique();

        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new { Id = SeedDataIds.EachUnitId, Code = "EA", Name = "عدد", IsActive = true, CreatedAtUtc = createdAt },
            new { Id = SeedDataIds.MeterUnitId, Code = "M", Name = "متر", IsActive = true, CreatedAtUtc = createdAt },
            new { Id = SeedDataIds.KilogramUnitId, Code = "KG", Name = "کیلوگرم", IsActive = true, CreatedAtUtc = createdAt },
            new { Id = SeedDataIds.LiterUnitId, Code = "L", Name = "لیتر", IsActive = false, CreatedAtUtc = createdAt },
            new { Id = SeedDataIds.PackageUnitId, Code = "PKG", Name = "بسته", IsActive = true, CreatedAtUtc = createdAt },
            new { Id = SeedDataIds.DeviceUnitId, Code = "DEV", Name = "دستگاه", IsActive = false, CreatedAtUtc = createdAt });
    }
}
