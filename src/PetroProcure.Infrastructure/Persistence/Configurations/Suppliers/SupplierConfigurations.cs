using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Suppliers;
using PetroProcure.Infrastructure.Persistence.Seeding;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Suppliers;

internal sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> b)
    {
        b.ToTable("Suppliers", DatabaseSchemas.Supplier);
        b.ConfigureAuditableEntity();
        b.Property(x => x.SupplierCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(300).IsRequired();
        b.Property(x => x.NationalId).HasMaxLength(50);
        b.Property(x => x.EconomicCode).HasMaxLength(50);
        b.Property(x => x.RegistrationNumber).HasMaxLength(50);
        b.Property(x => x.Phone).HasMaxLength(50);
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.Website).HasMaxLength(300);
        b.Property(x => x.Address).HasMaxLength(1000);
        b.Property(x => x.City).HasMaxLength(100);
        b.Property(x => x.Country).HasMaxLength(100);
        b.Property(x => x.PostalCode).HasMaxLength(50);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.SupplierType).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.BlacklistReason).HasMaxLength(1000);
        b.Property(x => x.Description).HasMaxLength(2000);
        b.HasIndex(x => x.SupplierCode).IsUnique();
        b.HasIndex(x => x.Name);
        b.HasIndex(x => x.NationalId);
        b.HasIndex(x => x.EconomicCode);
        b.HasIndex(x => x.IsActive);
        b.HasIndex(x => x.Status);
        b.HasMany(x => x.Contacts).WithOne(x => x.Supplier).HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.CategoryAssignments).WithOne(x => x.Supplier).HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Documents).WithOne(x => x.Supplier).HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Evaluations).WithOne(x => x.Supplier).HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Cascade);
        b.HasData(SupplierSeed.Suppliers);
    }
}

internal sealed class SupplierContactConfiguration : IEntityTypeConfiguration<SupplierContact>
{
    public void Configure(EntityTypeBuilder<SupplierContact> b)
    {
        b.ToTable("SupplierContacts", DatabaseSchemas.Supplier);
        b.ConfigureEntity();
        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Position).HasMaxLength(150);
        b.Property(x => x.Phone).HasMaxLength(50);
        b.Property(x => x.Mobile).HasMaxLength(50);
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.HasIndex(x => new { x.SupplierId, x.IsPrimary, x.IsActive });
        b.HasData(SupplierSeed.Contacts);
    }
}

internal sealed class SupplierCategoryConfiguration : IEntityTypeConfiguration<SupplierCategory>
{
    public void Configure(EntityTypeBuilder<SupplierCategory> b)
    {
        b.ToTable("SupplierCategories", DatabaseSchemas.Supplier);
        b.ConfigureEntity();
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
        b.HasIndex(x => x.Code).IsUnique();
        b.HasData(SupplierSeed.Categories);
    }
}

internal sealed class SupplierCategoryAssignmentConfiguration : IEntityTypeConfiguration<SupplierCategoryAssignment>
{
    public void Configure(EntityTypeBuilder<SupplierCategoryAssignment> b)
    {
        b.ToTable("SupplierCategoryAssignments", DatabaseSchemas.Supplier);
        b.ConfigureEntity();
        b.HasIndex(x => new { x.SupplierId, x.SupplierCategoryId }).IsUnique();
        b.HasOne(x => x.SupplierCategory).WithMany().HasForeignKey(x => x.SupplierCategoryId).OnDelete(DeleteBehavior.Restrict);
        b.HasData(SupplierSeed.Assignments);
    }
}

internal sealed class SupplierDocumentConfiguration : IEntityTypeConfiguration<SupplierDocument>
{
    public void Configure(EntityTypeBuilder<SupplierDocument> b)
    {
        b.ToTable("SupplierDocuments", DatabaseSchemas.Supplier);
        b.ConfigureEntity();
        b.Property(x => x.DocumentType).HasMaxLength(100).IsRequired();
        b.Property(x => x.OriginalFileName).HasMaxLength(260).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
    }
}

internal sealed class SupplierEvaluationConfiguration : IEntityTypeConfiguration<SupplierEvaluation>
{
    public void Configure(EntityTypeBuilder<SupplierEvaluation> b)
    {
        b.ToTable("SupplierEvaluations", DatabaseSchemas.Supplier);
        b.ConfigureEntity();
        b.Property(x => x.Score).HasPrecision(5, 2);
        b.Property(x => x.Result).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.Description).HasMaxLength(2000);
    }
}

internal static class SupplierSeed
{
    private static readonly DateTime CreatedAt = new(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);
    public static readonly Guid MechanicalCategoryId = Guid.Parse("a1000000-0000-0000-0000-000000000001");
    public static readonly Guid ElectricalCategoryId = Guid.Parse("a1000000-0000-0000-0000-000000000002");
    public static readonly Guid InstrumentCategoryId = Guid.Parse("a1000000-0000-0000-0000-000000000003");
    public static readonly Guid ChemicalCategoryId = Guid.Parse("a1000000-0000-0000-0000-000000000004");
    public static readonly Guid ContractorCategoryId = Guid.Parse("a1000000-0000-0000-0000-000000000005");
    public static readonly Guid Supplier1Id = Guid.Parse("a2000000-0000-0000-0000-000000000001");
    public static readonly Guid Supplier2Id = Guid.Parse("a2000000-0000-0000-0000-000000000002");
    public static readonly Guid Supplier3Id = Guid.Parse("a2000000-0000-0000-0000-000000000003");
    public static readonly Guid Supplier4Id = Guid.Parse("a2000000-0000-0000-0000-000000000004");
    public static readonly Guid Supplier5Id = Guid.Parse("a2000000-0000-0000-0000-000000000005");

    public static readonly SupplierCategory[] Categories =
    [
        new(MechanicalCategoryId, "MECH", "تجهیزات مکانیکی"),
        new(ElectricalCategoryId, "ELEC", "تجهیزات برقی"),
        new(InstrumentCategoryId, "INST", "ابزار دقیق"),
        new(ChemicalCategoryId, "CHEM", "مواد شیمیایی"),
        new(ContractorCategoryId, "CONT", "خدمات پیمانکاری")
    ];

    public static readonly object[] Suppliers =
    [
        Supplier(Supplier1Id, "SUP-0001", "تأمین صنعت جنوب", SupplierType.Distributor, "اهواز"),
        Supplier(Supplier2Id, "SUP-0002", "پارس تجهیز پالایش", SupplierType.Manufacturer, "تهران"),
        Supplier(Supplier3Id, "SUP-0003", "ابزار دقیق خلیج فارس", SupplierType.Distributor, "بوشهر"),
        Supplier(Supplier4Id, "SUP-0004", "شیمی گستر ایرانیان", SupplierType.Manufacturer, "شیراز"),
        Supplier(Supplier5Id, "SUP-0005", "پیمانکاران انرژی ساحل", SupplierType.Contractor, "بندرعباس")
    ];

    public static readonly SupplierContact[] Contacts =
    [
        Contact("a3000000-0000-0000-0000-000000000001", Supplier1Id, "علی رضایی", "مدیر فروش", "06100000001"),
        Contact("a3000000-0000-0000-0000-000000000002", Supplier2Id, "مریم احمدی", "کارشناس بازرگانی", "02100000002"),
        Contact("a3000000-0000-0000-0000-000000000003", Supplier3Id, "حسین کریمی", "مدیر حساب", "07700000003"),
        Contact("a3000000-0000-0000-0000-000000000004", Supplier4Id, "زهرا محمدی", "فروش سازمانی", "07100000004"),
        Contact("a3000000-0000-0000-0000-000000000005", Supplier5Id, "رضا امینی", "مدیر پروژه", "07600000005")
    ];

    public static readonly SupplierCategoryAssignment[] Assignments =
    [
        new(Guid.Parse("a4000000-0000-0000-0000-000000000001"), Supplier1Id, MechanicalCategoryId),
        new(Guid.Parse("a4000000-0000-0000-0000-000000000002"), Supplier2Id, MechanicalCategoryId),
        new(Guid.Parse("a4000000-0000-0000-0000-000000000003"), Supplier3Id, InstrumentCategoryId),
        new(Guid.Parse("a4000000-0000-0000-0000-000000000004"), Supplier4Id, ChemicalCategoryId),
        new(Guid.Parse("a4000000-0000-0000-0000-000000000005"), Supplier5Id, ContractorCategoryId)
    ];

    private static object Supplier(Guid id, string code, string name, SupplierType type, string city) => new
    {
        Id = id,
        SupplierCode = code,
        Name = name,
        NationalId = (string?)null,
        EconomicCode = (string?)null,
        RegistrationNumber = (string?)null,
        Phone = "00000000",
        Email = $"{code.ToLowerInvariant()}@example.local",
        Website = (string?)null,
        Address = (string?)null,
        City = city,
        Country = "ایران",
        PostalCode = (string?)null,
        Status = SupplierStatus.Active,
        SupplierType = type,
        IsActive = true,
        IsBlacklisted = false,
        BlacklistReason = (string?)null,
        Description = "داده نمونه تأمین‌کننده",
        CreatedAt,
        CreatedByUserId = IdentitySeedData.DefaultAdminUserId,
        UpdatedAt = (DateTime?)null,
        UpdatedByUserId = (Guid?)null,
        CreatedAtUtc = CreatedAt,
        CreatedBy = (string?)null,
        ModifiedAtUtc = (DateTime?)null,
        ModifiedBy = (string?)null
    };

    private static SupplierContact Contact(string id, Guid supplierId, string name, string position, string phone) =>
        new(Guid.Parse(id), supplierId, name, position, phone, phone.Replace("0", "9"), null, true, null);
}
