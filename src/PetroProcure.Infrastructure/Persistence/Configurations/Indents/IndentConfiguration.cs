using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Infrastructure.Persistence.Configurations;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Indents;

internal sealed class IndentConfiguration : IEntityTypeConfiguration<Indent>
{
    public void Configure(EntityTypeBuilder<Indent> builder)
    {
        builder.ToTable("Indents", DatabaseSchemas.Indent);
        builder.ConfigureAuditableEntity();
        builder.Property(indent => indent.IndentNumber).IsRequired().HasMaxLength(7).IsFixedLength();
        builder.Property(indent => indent.YearPart).IsRequired();
        builder.Property(indent => indent.TypeDigit).IsRequired();
        builder.Property(indent => indent.Sequence).IsRequired();
        builder.Property(indent => indent.IndentType).IsRequired();
        builder.Property(indent => indent.Title).IsRequired().HasMaxLength(300);
        builder.Property(indent => indent.CreatedAt).IsRequired();
        builder.Property(indent => indent.Status).IsRequired();
        builder.Property(indent => indent.Description).HasMaxLength(2000);
        builder.Property(indent => indent.SourceType).IsRequired();
        builder.Property(indent => indent.SourceDescription).HasMaxLength(500);

        builder.HasIndex(indent => indent.IndentNumber).IsUnique();
        builder.HasIndex(indent => new { indent.YearPart, indent.TypeDigit, indent.Sequence }).IsUnique();
        builder.HasOne<Department>().WithMany().HasForeignKey(indent => indent.RequestingDepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Department>().WithMany().HasForeignKey(indent => indent.ApplicantDepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(indent => indent.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(indent => indent.Items).WithOne().HasForeignKey(item => item.IndentId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(indent => indent.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
