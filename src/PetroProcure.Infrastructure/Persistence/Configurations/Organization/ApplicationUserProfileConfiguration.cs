using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Infrastructure.Persistence.Configurations;
using PetroProcure.Infrastructure.Persistence.Seeding;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Organization;

internal sealed class ApplicationUserProfileConfiguration : IEntityTypeConfiguration<ApplicationUserProfile>
{
    public void Configure(EntityTypeBuilder<ApplicationUserProfile> builder)
    {
        builder.ToTable("ApplicationUserProfiles", DatabaseSchemas.Organization);
        builder.ConfigureAuditableEntity();

        builder.Property(user => user.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(user => user.Email)
            .HasMaxLength(320);

        builder.Property(user => user.IsActive)
            .IsRequired();

        builder.HasIndex(user => user.Email);

        builder.HasData(new
        {
            Id = IdentitySeedData.DefaultAdminProfileId,
            DisplayName = "مدیر سامانه",
            Email = IdentitySeedData.DefaultAdminEmail,
            IsActive = true,
            CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
