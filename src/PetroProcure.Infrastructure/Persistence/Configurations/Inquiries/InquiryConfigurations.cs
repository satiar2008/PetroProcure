using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Inquiries;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Inquiries;

internal sealed class InquiryConfiguration : IEntityTypeConfiguration<Inquiry>
{
    public void Configure(EntityTypeBuilder<Inquiry> b)
    {
        b.ToTable("Inquiries", DatabaseSchemas.Inquiry);
        b.ConfigureEntity();
        b.Property(x => x.InquiryNumber).HasMaxLength(50).IsRequired();
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.InquiryType).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.CancellationReason).HasMaxLength(1000);
        b.HasIndex(x => x.InquiryNumber).IsUnique();
        b.HasIndex(x => x.PurchaseFileId);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.CreatedAt);
        b.HasIndex(x => x.DeadlineDate);
        b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.InquiryId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Suppliers).WithOne().HasForeignKey(x => x.InquiryId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Quotes).WithOne().HasForeignKey(x => x.InquiryId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Documents).WithOne().HasForeignKey(x => x.InquiryId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class InquiryItemConfiguration : IEntityTypeConfiguration<InquiryItem>
{
    public void Configure(EntityTypeBuilder<InquiryItem> b)
    {
        b.ToTable("InquiryItems", DatabaseSchemas.Inquiry);
        b.ConfigureEntity();
        b.Property(x => x.MescCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.MescGeneralGroupCode).HasMaxLength(20).IsRequired();
        b.Property(x => x.GeneralDescription).HasMaxLength(500).IsRequired();
        b.Property(x => x.SpecificDescription).HasMaxLength(500).IsRequired();
        b.Property(x => x.RequestedQuantity).HasPrecision(18, 3);
        b.Property(x => x.TechnicalDescription).HasMaxLength(2000);
    }
}

internal sealed class InquirySupplierConfiguration : IEntityTypeConfiguration<InquirySupplier>
{
    public void Configure(EntityTypeBuilder<InquirySupplier> b)
    {
        b.ToTable("InquirySuppliers", DatabaseSchemas.Inquiry);
        b.ConfigureEntity();
        b.Property(x => x.SupplierCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.SupplierName).HasMaxLength(300).IsRequired();
        b.Property(x => x.ContactName).HasMaxLength(200);
        b.Property(x => x.ContactEmail).HasMaxLength(200);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.DeclineReason).HasMaxLength(1000);
        b.HasIndex(x => x.SupplierId);
    }
}

internal sealed class SupplierQuoteConfiguration : IEntityTypeConfiguration<SupplierQuote>
{
    public void Configure(EntityTypeBuilder<SupplierQuote> b)
    {
        b.ToTable("SupplierQuotes", DatabaseSchemas.Inquiry);
        b.ConfigureEntity();
        b.Property(x => x.QuoteNumber).HasMaxLength(100);
        b.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        b.Property(x => x.DeliveryTerms).HasMaxLength(500);
        b.Property(x => x.PaymentTerms).HasMaxLength(500);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.TaxAmount).HasPrecision(18, 2);
        b.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        b.Property(x => x.FinalAmount).HasPrecision(18, 2);
        b.Property(x => x.TechnicalNote).HasMaxLength(2000);
        b.Property(x => x.CommercialNote).HasMaxLength(2000);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.SelectionReason).HasMaxLength(1000);
        b.HasIndex(x => new { x.InquiryId, x.SupplierId });
        b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.SupplierQuoteId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SupplierQuoteItemConfiguration : IEntityTypeConfiguration<SupplierQuoteItem>
{
    public void Configure(EntityTypeBuilder<SupplierQuoteItem> b)
    {
        b.ToTable("SupplierQuoteItems", DatabaseSchemas.Inquiry);
        b.ConfigureEntity();
        b.Property(x => x.MescCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.MescGeneralGroupCode).HasMaxLength(20).IsRequired();
        b.Property(x => x.GeneralDescription).HasMaxLength(500).IsRequired();
        b.Property(x => x.SpecificDescription).HasMaxLength(500).IsRequired();
        b.Property(x => x.Quantity).HasPrecision(18, 3);
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.TotalPrice).HasPrecision(18, 2);
        b.Property(x => x.TechnicalComplianceStatus).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.TechnicalNote).HasMaxLength(2000);
    }
}

internal sealed class InquiryDocumentConfiguration : IEntityTypeConfiguration<InquiryDocument>
{
    public void Configure(EntityTypeBuilder<InquiryDocument> b)
    {
        b.ToTable("InquiryDocuments", DatabaseSchemas.Inquiry);
        b.ConfigureEntity();
        b.Property(x => x.DocumentType).HasMaxLength(100).IsRequired();
        b.Property(x => x.OriginalFileName).HasMaxLength(260);
        b.Property(x => x.Description).HasMaxLength(1000);
    }
}

internal sealed class InquirySequenceConfiguration : IEntityTypeConfiguration<InquirySequence>
{
    public void Configure(EntityTypeBuilder<InquirySequence> b)
    {
        b.ToTable("InquirySequences", DatabaseSchemas.Inquiry);
        b.ConfigureEntity();
        b.HasIndex(x => x.Year).IsUnique();
    }
}
