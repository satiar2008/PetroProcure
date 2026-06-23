using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Infrastructure.Persistence.Configurations;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Organization;

internal sealed class UserDepartmentConfiguration : IEntityTypeConfiguration<UserDepartment>
{
    public void Configure(EntityTypeBuilder<UserDepartment> builder)
    {
        builder.ToTable("UserDepartments", DatabaseSchemas.Organization);
        builder.ConfigureAuditableEntity();

        builder.Property(userDepartment => userDepartment.UserProfileId).IsRequired();
        builder.Property(userDepartment => userDepartment.DepartmentId).IsRequired();
        builder.Property(userDepartment => userDepartment.IsPrimary).IsRequired();

        builder.HasIndex(userDepartment => new { userDepartment.UserProfileId, userDepartment.DepartmentId })
            .IsUnique();

        builder.HasOne<ApplicationUserProfile>()
            .WithMany()
            .HasForeignKey(userDepartment => userDepartment.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Department>()
            .WithMany()
            .HasForeignKey(userDepartment => userDepartment.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
