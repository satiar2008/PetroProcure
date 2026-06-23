using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Common;

namespace PetroProcure.Infrastructure.Persistence.Configurations;

internal static class EntityConfigurationExtensions
{
    public static void ConfigureEntity<T>(this EntityTypeBuilder<T> builder)
        where T : Entity<Guid>
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedNever();
        builder.Ignore(entity => entity.DomainEvents);
    }

    public static void ConfigureAuditableEntity<T>(this EntityTypeBuilder<T> builder)
        where T : AuditableEntity<Guid>
    {
        builder.ConfigureEntity();
        builder.Property(entity => entity.CreatedAtUtc).IsRequired();
        builder.Property(entity => entity.CreatedBy).HasMaxLength(100);
        builder.Property(entity => entity.ModifiedBy).HasMaxLength(100);
    }
}
