using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Infrastructure.Persistence.Configurations;
using PetroProcure.Infrastructure.Persistence.Seeding;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Organization;

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments", DatabaseSchemas.Organization);
        builder.ConfigureAuditableEntity();

        builder.Property(department => department.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(department => department.Type)
            .IsRequired();

        builder.Property(department => department.IsActive)
            .IsRequired();

        builder.HasIndex(department => department.Type);
        builder.HasIndex(department => department.Name).IsUnique();

        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new
            {
                Id = SeedDataIds.PurchaseDepartmentId,
                Name = "واحد خرید",
                Type = DepartmentType.PurchaseDepartment,
                IsActive = true,
                CreatedAtUtc = createdAt
            },
            new
            {
                Id = SeedDataIds.OrdersAndInventoryControlId,
                Name = "سفارشات و کنترل موجودی",
                Type = DepartmentType.OrdersAndInventoryControl,
                IsActive = true,
                CreatedAtUtc = createdAt
            },
            new
            {
                Id = SeedDataIds.WarehouseId,
                Name = "انبار",
                Type = DepartmentType.Warehouse,
                IsActive = true,
                CreatedAtUtc = createdAt
            },
            new
            {
                Id = SeedDataIds.ApplicantId,
                Name = "متقاضی",
                Type = DepartmentType.Applicant,
                IsActive = true,
                CreatedAtUtc = createdAt
            },
            new
            {
                Id = SeedDataIds.TenderCommissionId,
                Name = "کمیسیون مناقصه",
                Type = DepartmentType.TenderCommission,
                IsActive = true,
                CreatedAtUtc = createdAt
            });
    }
}
