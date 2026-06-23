using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Items;
using PetroProcure.Infrastructure.Persistence.Configurations;
using PetroProcure.Infrastructure.Persistence.Seeding;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Items;

internal sealed class MescGeneralGroupConfiguration : IEntityTypeConfiguration<MescGeneralGroup>
{
    public void Configure(EntityTypeBuilder<MescGeneralGroup> builder)
    {
        builder.ToTable("MescGeneralGroups", DatabaseSchemas.Item);
        builder.ConfigureAuditableEntity();

        builder.Property(group => group.Code)
            .IsRequired()
            .HasMaxLength(6);

        builder.Property(group => group.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(group => group.IsActive)
            .IsRequired();

        builder.HasIndex(group => group.Code).IsUnique();
        builder.HasAlternateKey(group => group.Code);

        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new
            {
                Id = SeedDataIds.PipeFittingsGroupId,
                Code = "123456",
                Description = "لوله و اتصالات عمومی",
                IsActive = true,
                CreatedAtUtc = createdAt
            },
            new
            {
                Id = SeedDataIds.ValvesGroupId,
                Code = "223344",
                Description = "شیرآلات صنعتی",
                IsActive = true,
                CreatedAtUtc = createdAt
            },
            new
            {
                Id = SeedDataIds.PumpsGroupId,
                Code = "334455",
                Description = "پمپ‌ها و تجهیزات دوار",
                IsActive = true,
                CreatedAtUtc = createdAt
            });
    }
}
