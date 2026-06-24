using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Tenders;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Tenders;

internal sealed class TenderConfiguration : IEntityTypeConfiguration<Tender>
{
    public void Configure(EntityTypeBuilder<Tender> b)
    {
        b.ToTable("Tenders", DatabaseSchemas.Tender);
        b.ConfigureEntity();
        b.Property(x => x.TenderNumber).HasMaxLength(50).IsRequired();
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.TenderType).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.CancellationReason).HasMaxLength(1000);
        b.HasIndex(x => x.TenderNumber).IsUnique();
        b.HasIndex(x => x.PurchaseFileId);
        b.HasIndex(x => x.SourceInquiryId);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.CreatedAt);
        b.HasIndex(x => x.SubmissionDeadline);
        b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.TenderId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Participants).WithOne().HasForeignKey(x => x.TenderId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Stages).WithOne().HasForeignKey(x => x.TenderId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Bids).WithOne().HasForeignKey(x => x.TenderId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Evaluations).WithOne().HasForeignKey(x => x.TenderId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Decisions).WithOne().HasForeignKey(x => x.TenderId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Documents).WithOne().HasForeignKey(x => x.TenderId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class TenderItemConfiguration : IEntityTypeConfiguration<TenderItem>
{
    public void Configure(EntityTypeBuilder<TenderItem> b)
    {
        b.ToTable("TenderItems", DatabaseSchemas.Tender);
        b.ConfigureEntity();
        b.Property(x => x.MescCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.MescGeneralGroupCode).HasMaxLength(20).IsRequired();
        b.Property(x => x.GeneralDescription).HasMaxLength(500).IsRequired();
        b.Property(x => x.SpecificDescription).HasMaxLength(500).IsRequired();
        b.Property(x => x.Quantity).HasPrecision(18, 3);
        b.Property(x => x.TechnicalDescription).HasMaxLength(2000);
    }
}

internal sealed class TenderParticipantConfiguration : IEntityTypeConfiguration<TenderParticipant>
{
    public void Configure(EntityTypeBuilder<TenderParticipant> b)
    {
        b.ToTable("TenderParticipants", DatabaseSchemas.Tender);
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

internal sealed class TenderStageConfiguration : IEntityTypeConfiguration<TenderStage>
{
    public void Configure(EntityTypeBuilder<TenderStage> b)
    {
        b.ToTable("TenderStages", DatabaseSchemas.Tender);
        b.ConfigureEntity();
        b.Property(x => x.StageType).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.Notes).HasMaxLength(2000);
    }
}

internal sealed class TenderBidConfiguration : IEntityTypeConfiguration<TenderBid>
{
    public void Configure(EntityTypeBuilder<TenderBid> b)
    {
        b.ToTable("TenderBids", DatabaseSchemas.Tender);
        b.ConfigureEntity();
        b.Property(x => x.BidNumber).HasMaxLength(100);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.TechnicalScore).HasPrecision(18, 2);
        b.Property(x => x.CommercialScore).HasPrecision(18, 2);
        b.Property(x => x.FinalScore).HasPrecision(18, 2);
        b.Property(x => x.Currency).HasMaxLength(10);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.FinalAmount).HasPrecision(18, 2);
        b.Property(x => x.DeliveryTerms).HasMaxLength(500);
        b.Property(x => x.PaymentTerms).HasMaxLength(500);
        b.Property(x => x.Notes).HasMaxLength(2000);
        b.HasIndex(x => new { x.TenderId, x.SupplierId });
        b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.TenderBidId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class TenderBidItemConfiguration : IEntityTypeConfiguration<TenderBidItem>
{
    public void Configure(EntityTypeBuilder<TenderBidItem> b)
    {
        b.ToTable("TenderBidItems", DatabaseSchemas.Tender);
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

internal sealed class TenderEvaluationConfiguration : IEntityTypeConfiguration<TenderEvaluation>
{
    public void Configure(EntityTypeBuilder<TenderEvaluation> b)
    {
        b.ToTable("TenderEvaluations", DatabaseSchemas.Tender);
        b.ConfigureEntity();
        b.Property(x => x.EvaluationType).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.Result).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.Score).HasPrecision(18, 2);
        b.Property(x => x.Notes).HasMaxLength(2000);
    }
}

internal sealed class TenderDecisionConfiguration : IEntityTypeConfiguration<TenderDecision>
{
    public void Configure(EntityTypeBuilder<TenderDecision> b)
    {
        b.ToTable("TenderDecisions", DatabaseSchemas.Tender);
        b.ConfigureEntity();
        b.Property(x => x.DecisionType).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.Reason).HasMaxLength(1000);
        b.Property(x => x.Notes).HasMaxLength(2000);
    }
}

internal sealed class TenderDocumentConfiguration : IEntityTypeConfiguration<TenderDocument>
{
    public void Configure(EntityTypeBuilder<TenderDocument> b)
    {
        b.ToTable("TenderDocuments", DatabaseSchemas.Tender);
        b.ConfigureEntity();
        b.Property(x => x.DocumentType).HasMaxLength(100).IsRequired();
        b.Property(x => x.OriginalFileName).HasMaxLength(260);
        b.Property(x => x.Description).HasMaxLength(1000);
    }
}

internal sealed class TenderSequenceConfiguration : IEntityTypeConfiguration<TenderSequence>
{
    public void Configure(EntityTypeBuilder<TenderSequence> b)
    {
        b.ToTable("TenderSequences", DatabaseSchemas.Tender);
        b.ConfigureEntity();
        b.HasIndex(x => x.Year).IsUnique();
    }
}
