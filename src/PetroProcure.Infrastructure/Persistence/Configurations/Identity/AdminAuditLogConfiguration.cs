using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Infrastructure.Identity;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Identity;

internal sealed class AdminAuditLogConfiguration : IEntityTypeConfiguration<AdminAuditLog>
{
    public void Configure(EntityTypeBuilder<AdminAuditLog> b)
    {
        b.ToTable("AdminAuditLogs", DatabaseSchemas.Identity);
        b.HasKey(x => x.Id);
        b.Property(x => x.Action).IsRequired().HasMaxLength(120);
        b.Property(x => x.EntityType).IsRequired().HasMaxLength(120);
        b.Property(x => x.EntityId).HasMaxLength(120);
        b.Property(x => x.Summary).HasMaxLength(1000);
        b.HasIndex(x => new { x.EntityType, x.CreatedAt });
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.SetNull);
    }
}
