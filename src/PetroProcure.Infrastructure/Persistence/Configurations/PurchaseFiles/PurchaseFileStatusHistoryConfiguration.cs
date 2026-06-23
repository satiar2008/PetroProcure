using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Infrastructure.Persistence.Configurations;

namespace PetroProcure.Infrastructure.Persistence.Configurations.PurchaseFiles;

internal sealed class PurchaseFileStatusHistoryConfiguration : IEntityTypeConfiguration<PurchaseFileStatusHistory>
{
    public void Configure(EntityTypeBuilder<PurchaseFileStatusHistory> builder)
    {
        builder.ToTable("PurchaseFileStatusHistories", DatabaseSchemas.Purchase);
        builder.ConfigureEntity();
        builder.Property(history => history.FromStatus).IsRequired();
        builder.Property(history => history.ToStatus).IsRequired();
        builder.Property(history => history.ChangedAt).IsRequired();
        builder.Property(history => history.Reason).HasMaxLength(1000);
        builder.HasIndex(history => history.PurchaseFileId);
        builder.HasIndex(history => history.ChangedAt);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(history => history.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Department>().WithMany().HasForeignKey(history => history.DepartmentId).OnDelete(DeleteBehavior.Restrict);
    }
}
