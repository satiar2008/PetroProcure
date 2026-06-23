using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Infrastructure.Persistence.Seeding;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Identity;

internal sealed class IdentityConfiguration :
    IEntityTypeConfiguration<ApplicationUser>,
    IEntityTypeConfiguration<IdentityRole<Guid>>,
    IEntityTypeConfiguration<IdentityUserRole<Guid>>,
    IEntityTypeConfiguration<IdentityUserClaim<Guid>>,
    IEntityTypeConfiguration<IdentityUserLogin<Guid>>,
    IEntityTypeConfiguration<IdentityRoleClaim<Guid>>,
    IEntityTypeConfiguration<IdentityUserToken<Guid>>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users", DatabaseSchemas.Identity);

        builder.Property(user => user.UserProfileId);

        builder.HasIndex(user => user.UserProfileId)
            .IsUnique()
            .HasFilter("[UserProfileId] IS NOT NULL");

        builder.HasOne<PetroProcure.Domain.Modules.Organization.ApplicationUserProfile>()
            .WithMany()
            .HasForeignKey(user => user.UserProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasData(IdentitySeedData.AdminUser);
    }

    public void Configure(EntityTypeBuilder<IdentityRole<Guid>> builder)
    {
        builder.ToTable("Roles", DatabaseSchemas.Identity);
        builder.HasData(IdentitySeedData.Roles);
    }

    public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> builder)
    {
        builder.ToTable("UserRoles", DatabaseSchemas.Identity);
        builder.HasData(IdentitySeedData.AdminUserRole);
    }

    public void Configure(EntityTypeBuilder<IdentityUserClaim<Guid>> builder)
    {
        builder.ToTable("UserClaims", DatabaseSchemas.Identity);
    }

    public void Configure(EntityTypeBuilder<IdentityUserLogin<Guid>> builder)
    {
        builder.ToTable("UserLogins", DatabaseSchemas.Identity);
    }

    public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder)
    {
        builder.ToTable("RoleClaims", DatabaseSchemas.Identity);
    }

    public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> builder)
    {
        builder.ToTable("UserTokens", DatabaseSchemas.Identity);
    }
}
