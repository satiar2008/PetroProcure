using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Security;
using PetroProcure.Infrastructure.Persistence.Configurations;
using PetroProcure.Infrastructure.Persistence.Seeding;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Security;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions", DatabaseSchemas.Identity);
        builder.ConfigureAuditableEntity();

        builder.Property(permission => permission.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(permission => permission.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(permission => permission.IsActive).IsRequired();

        builder.HasIndex(permission => permission.Name).IsUnique();

        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(IdentitySeedData.Permissions.Select(permission => new
        {
            permission.Id,
            permission.Name,
            permission.Description,
            permission.IsActive,
            CreatedAtUtc = createdAt
        }));
    }
}
