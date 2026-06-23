using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Documents;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Infrastructure.Persistence.Configurations;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Documents;

internal sealed class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.ToTable("DocumentVersions", DatabaseSchemas.Document);
        builder.ConfigureEntity();
        builder.Property(version => version.VersionNo).IsRequired();
        builder.Property(version => version.StoredFileName).IsRequired().HasMaxLength(260);
        builder.Property(version => version.RelativePath).IsRequired().HasMaxLength(1000);
        builder.Property(version => version.Hash).IsRequired().HasMaxLength(64);
        builder.Property(version => version.CreatedAt).IsRequired();
        builder.HasIndex(version => new { version.FileDocumentId, version.VersionNo }).IsUnique();
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(version => version.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
