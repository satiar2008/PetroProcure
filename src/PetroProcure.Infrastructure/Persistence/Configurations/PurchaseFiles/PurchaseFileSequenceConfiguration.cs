using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Infrastructure.Persistence.Configurations;

namespace PetroProcure.Infrastructure.Persistence.Configurations.PurchaseFiles;

internal sealed class PurchaseFileSequenceConfiguration : IEntityTypeConfiguration<PurchaseFileSequence>
{
    public void Configure(EntityTypeBuilder<PurchaseFileSequence> builder)
    {
        builder.ToTable("PurchaseFileSequences", DatabaseSchemas.Purchase);
        builder.ConfigureEntity();
        builder.Property(sequence => sequence.Year).IsRequired();
        builder.Property(sequence => sequence.LastSequence).IsRequired();
        builder.HasIndex(sequence => sequence.Year).IsUnique();
    }
}
