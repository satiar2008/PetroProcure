using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Documents;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Infrastructure.Persistence.Configurations;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Documents;

internal sealed class FileDocumentConfiguration : IEntityTypeConfiguration<FileDocument>
{
    public void Configure(EntityTypeBuilder<FileDocument> builder)
    {
        builder.ToTable("FileDocuments", DatabaseSchemas.Document);
        builder.ConfigureAuditableEntity();
        builder.Property(document => document.DocumentType).IsRequired();
        builder.Property(document => document.OriginalFileName).IsRequired().HasMaxLength(260);
        builder.Property(document => document.StoredFileName).IsRequired().HasMaxLength(260);
        builder.Property(document => document.RelativePath).IsRequired().HasMaxLength(1000);
        builder.Property(document => document.Extension).IsRequired().HasMaxLength(30);
        builder.Property(document => document.MimeType).IsRequired().HasMaxLength(200);
        builder.Property(document => document.Hash).IsRequired().HasMaxLength(64);
        builder.Property(document => document.VersionNo).IsRequired();
        builder.Property(document => document.UploadedAt).IsRequired();
        builder.Property(document => document.IsDeleted).IsRequired();
        builder.Property(document => document.Description).HasMaxLength(2000);
        builder.HasIndex(document => document.PurchaseFileId);
        builder.HasIndex(document => new { document.PurchaseFileId, document.DocumentType });
        builder.HasOne<PurchaseFile>().WithMany().HasForeignKey(document => document.PurchaseFileId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Department>().WithMany().HasForeignKey(document => document.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(document => document.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(document => document.Versions).WithOne().HasForeignKey(version => version.FileDocumentId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(document => document.Versions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
