using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Items;
using PetroProcure.Infrastructure.Persistence.Configurations;
using PetroProcure.Infrastructure.Persistence.Seeding;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Items;

internal sealed class MescItemConfiguration : IEntityTypeConfiguration<MescItem>
{
    public void Configure(EntityTypeBuilder<MescItem> builder)
    {
        builder.ToTable("MescItems", DatabaseSchemas.Item);
        builder.ConfigureAuditableEntity();

        builder.Property(item => item.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(item => item.GeneralGroupCode)
            .IsRequired()
            .HasMaxLength(6);

        builder.Property(item => item.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(item => item.UnitOfMeasure)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(item => item.IsActive)
            .IsRequired();

        builder.HasIndex(item => item.Code).IsUnique();
        builder.HasIndex(item => item.GeneralGroupCode);

        builder.HasOne(item => item.GeneralGroup)
            .WithMany()
            .HasForeignKey(item => item.GeneralGroupCode)
            .HasPrincipalKey(group => group.Code)
            .OnDelete(DeleteBehavior.Restrict);

        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new
            {
                Id = SeedDataIds.PipeItemId,
                Code = "1234560001",
                GeneralGroupCode = "123456",
                Description = "لوله فولادی عمومی",
                UnitOfMeasure = "M",
                IsActive = true,
                CreatedAtUtc = createdAt
            },
            new
            {
                Id = SeedDataIds.ElbowItemId,
                Code = "1234560002",
                GeneralGroupCode = "123456",
                Description = "زانو فولادی عمومی",
                UnitOfMeasure = "EA",
                IsActive = true,
                CreatedAtUtc = createdAt
            },
            new
            {
                Id = SeedDataIds.GateValveItemId,
                Code = "2233440001",
                GeneralGroupCode = "223344",
                Description = "شیر کشویی صنعتی",
                UnitOfMeasure = "EA",
                IsActive = true,
                CreatedAtUtc = createdAt
            },
            new
            {
                Id = SeedDataIds.CentrifugalPumpItemId,
                Code = "3344550001",
                GeneralGroupCode = "334455",
                Description = "پمپ سانتریفیوژ عمومی",
                UnitOfMeasure = "DEV",
                IsActive = true,
                CreatedAtUtc = createdAt
            },
            new
            {
                Id = SeedDataIds.FlangeItemId, Code = "1234560003", GeneralGroupCode = "123456",
                Description = "فلنج فولادی", UnitOfMeasure = "EA", IsActive = true, CreatedAtUtc = createdAt
            },
            new
            {
                Id = SeedDataIds.TeeItemId, Code = "1234560004", GeneralGroupCode = "123456",
                Description = "سه راهی فولادی", UnitOfMeasure = "EA", IsActive = true, CreatedAtUtc = createdAt
            },
            new
            {
                Id = SeedDataIds.BallValveItemId, Code = "2233440002", GeneralGroupCode = "223344",
                Description = "شیر توپی صنعتی", UnitOfMeasure = "EA", IsActive = true, CreatedAtUtc = createdAt
            },
            new
            {
                Id = SeedDataIds.CheckValveItemId, Code = "2233440003", GeneralGroupCode = "223344",
                Description = "شیر یک طرفه صنعتی", UnitOfMeasure = "EA", IsActive = true, CreatedAtUtc = createdAt
            },
            new
            {
                Id = SeedDataIds.GearPumpItemId, Code = "3344550002", GeneralGroupCode = "334455",
                Description = "پمپ دنده‌ای", UnitOfMeasure = "DEV", IsActive = true, CreatedAtUtc = createdAt
            },
            new
            {
                Id = SeedDataIds.PumpSealItemId, Code = "3344550003", GeneralGroupCode = "334455",
                Description = "مکانیکال سیل پمپ", UnitOfMeasure = "EA", IsActive = true, CreatedAtUtc = createdAt
            });
    }
}
