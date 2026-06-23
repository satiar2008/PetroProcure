using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Infrastructure.Persistence.Configurations;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Indents;

internal sealed class IndentSequenceConfiguration : IEntityTypeConfiguration<IndentSequence>
{
    public void Configure(EntityTypeBuilder<IndentSequence> builder)
    {
        builder.ToTable("IndentSequences", DatabaseSchemas.Indent);
        builder.ConfigureEntity();
        builder.Property(sequence => sequence.YearPart).IsRequired();
        builder.Property(sequence => sequence.TypeDigit).IsRequired();
        builder.Property(sequence => sequence.LastSequence).IsRequired();
        builder.HasIndex(sequence => new { sequence.YearPart, sequence.TypeDigit }).IsUnique();
    }
}
