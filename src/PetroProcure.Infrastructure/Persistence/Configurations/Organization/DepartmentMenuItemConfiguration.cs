using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Infrastructure.Persistence.Configurations;
using PetroProcure.Infrastructure.Persistence.Seeding;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Organization;

internal sealed class DepartmentMenuItemConfiguration : IEntityTypeConfiguration<DepartmentMenuItem>
{
    public void Configure(EntityTypeBuilder<DepartmentMenuItem> builder)
    {
        builder.ToTable("DepartmentMenuItems", DatabaseSchemas.Organization);
        builder.ConfigureAuditableEntity();

        builder.Property(item => item.DepartmentType).IsRequired();

        builder.Property(item => item.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(item => item.Route)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(item => item.RequiredPermission)
            .HasMaxLength(150);

        builder.Property(item => item.Order).IsRequired();
        builder.Property(item => item.IsVisible).IsRequired();

        builder.HasIndex(item => item.DepartmentType);
        builder.HasIndex(item => item.RequiredPermission);

        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(IdentitySeedData.DepartmentMenuItems.Select(item => new
        {
            item.Id,
            item.DepartmentType,
            item.Title,
            item.Route,
            item.RequiredPermission,
            item.Order,
            item.IsVisible,
            CreatedAtUtc = createdAt
        }));
    }
}
