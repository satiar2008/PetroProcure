using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Infrastructure.Persistence.Configurations;

namespace PetroProcure.Infrastructure.Persistence.Configurations.PurchaseFiles;

internal sealed class PurchaseFileNoteConfiguration : IEntityTypeConfiguration<PurchaseFileNote>
{
    public void Configure(EntityTypeBuilder<PurchaseFileNote> builder)
    {
        builder.ToTable("PurchaseFileNotes", DatabaseSchemas.Purchase);
        builder.ConfigureEntity();
        builder.Property(note => note.NoteText).IsRequired().HasMaxLength(2000);
        builder.Property(note => note.CreatedAt).IsRequired();
        builder.Property(note => note.IsInternal).IsRequired();
        builder.HasIndex(note => note.PurchaseFileId);
        builder.HasOne<Department>().WithMany().HasForeignKey(note => note.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(note => note.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
