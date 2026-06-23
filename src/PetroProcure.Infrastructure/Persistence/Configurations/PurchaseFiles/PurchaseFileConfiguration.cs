using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Infrastructure.Persistence.Configurations;

namespace PetroProcure.Infrastructure.Persistence.Configurations.PurchaseFiles;

internal sealed class PurchaseFileConfiguration : IEntityTypeConfiguration<PurchaseFile>
{
    public void Configure(EntityTypeBuilder<PurchaseFile> builder)
    {
        builder.ToTable("PurchaseFiles", DatabaseSchemas.Purchase);
        builder.ConfigureAuditableEntity();
        builder.Property(file => file.FileNumber).IsRequired().HasMaxLength(14);
        builder.Property(file => file.Title).IsRequired().HasMaxLength(300);
        builder.Property(file => file.Description).HasMaxLength(2000);
        builder.Property(file => file.Status).IsRequired();
        builder.Property(file => file.Priority).IsRequired();
        builder.Property(file => file.CreatedAt).IsRequired();
        builder.HasIndex(file => file.FileNumber).IsUnique();
        builder.HasIndex(file => file.SourceIndentId).IsUnique().HasFilter("[SourceIndentId] IS NOT NULL");
        builder.HasIndex(file => file.Status);
        builder.HasOne<Indent>().WithMany().HasForeignKey(file => file.SourceIndentId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<Department>().WithMany().HasForeignKey(file => file.PurchaseDepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Department>().WithMany().HasForeignKey(file => file.CurrentDepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(file => file.ResponsibleUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(file => file.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(file => file.Items).WithOne().HasForeignKey(item => item.PurchaseFileId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(file => file.StatusHistory).WithOne().HasForeignKey(history => history.PurchaseFileId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(file => file.Notes).WithOne().HasForeignKey(note => note.PurchaseFileId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(file => file.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(file => file.StatusHistory).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(file => file.Notes).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
