using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Documents;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.Suppliers;
using PetroProcure.Domain.Modules.TenderCommission;
using PetroProcure.Domain.Modules.Tenders;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Commission;

internal sealed class TenderCommissionSessionConfiguration : IEntityTypeConfiguration<TenderCommissionSession>
{
    public void Configure(EntityTypeBuilder<TenderCommissionSession> b)
    {
        b.ToTable("TenderCommissionSessions", DatabaseSchemas.Commission);
        b.ConfigureEntity();
        b.Property(x => x.SessionNumber).HasMaxLength(50).IsRequired();
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.Location).HasMaxLength(300);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.CancellationReason).HasMaxLength(1000);
        b.HasIndex(x => x.SessionNumber).IsUnique();
        b.HasIndex(x => x.TenderId);
        b.HasIndex(x => x.PurchaseFileId);
        b.HasIndex(x => x.SessionDate);
        b.HasIndex(x => x.Status);
        b.HasOne<Tender>().WithMany().HasForeignKey(x => x.TenderId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<PurchaseFile>().WithMany().HasForeignKey(x => x.PurchaseFileId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Members).WithOne().HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.AgendaItems).WithOne().HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Minutes).WithOne().HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Decisions).WithOne().HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Attachments).WithOne().HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class TenderCommissionMemberConfiguration : IEntityTypeConfiguration<TenderCommissionMember>
{
    public void Configure(EntityTypeBuilder<TenderCommissionMember> b)
    {
        b.ToTable("TenderCommissionMembers", DatabaseSchemas.Commission);
        b.ConfigureEntity();
        b.Property(x => x.FullNameSnapshot).HasMaxLength(250).IsRequired();
        b.Property(x => x.PositionSnapshot).HasMaxLength(200);
        b.Property(x => x.Role).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.AttendanceStatus).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.VoteStatus).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.VoteNote).HasMaxLength(1000);
        b.HasIndex(x => x.UserId);
    }
}

internal sealed class TenderCommissionAgendaItemConfiguration : IEntityTypeConfiguration<TenderCommissionAgendaItem>
{
    public void Configure(EntityTypeBuilder<TenderCommissionAgendaItem> b)
    {
        b.ToTable("TenderCommissionAgendaItems", DatabaseSchemas.Commission);
        b.ConfigureEntity();
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.Notes).HasMaxLength(2000);
        b.HasOne<TenderBid>().WithMany().HasForeignKey(x => x.RelatedTenderBidId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Supplier>().WithMany().HasForeignKey(x => x.RelatedSupplierId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class TenderCommissionMinuteConfiguration : IEntityTypeConfiguration<TenderCommissionMinute>
{
    public void Configure(EntityTypeBuilder<TenderCommissionMinute> b)
    {
        b.ToTable("TenderCommissionMinutes", DatabaseSchemas.Commission);
        b.ConfigureEntity();
        b.Property(x => x.Text).HasMaxLength(4000).IsRequired();
        b.HasIndex(x => x.AgendaItemId);
        b.HasOne<TenderCommissionAgendaItem>().WithMany().HasForeignKey(x => x.AgendaItemId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class TenderCommissionDecisionConfiguration : IEntityTypeConfiguration<TenderCommissionDecision>
{
    public void Configure(EntityTypeBuilder<TenderCommissionDecision> b)
    {
        b.ToTable("TenderCommissionDecisions", DatabaseSchemas.Commission);
        b.ConfigureEntity();
        b.Property(x => x.DecisionType).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.DecisionText).HasMaxLength(4000).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(1000);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        b.HasIndex(x => x.SelectedSupplierId);
        b.HasIndex(x => x.SelectedTenderBidId);
        b.HasOne<Tender>().WithMany().HasForeignKey(x => x.TenderId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<TenderBid>().WithMany().HasForeignKey(x => x.SelectedTenderBidId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Supplier>().WithMany().HasForeignKey(x => x.SelectedSupplierId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class TenderCommissionAttachmentConfiguration : IEntityTypeConfiguration<TenderCommissionAttachment>
{
    public void Configure(EntityTypeBuilder<TenderCommissionAttachment> b)
    {
        b.ToTable("TenderCommissionAttachments", DatabaseSchemas.Commission);
        b.ConfigureEntity();
        b.Property(x => x.DocumentType).HasMaxLength(100).IsRequired();
        b.Property(x => x.OriginalFileName).HasMaxLength(260);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.HasOne<FileDocument>().WithMany().HasForeignKey(x => x.FileDocumentId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class TenderCommissionSessionSequenceConfiguration : IEntityTypeConfiguration<TenderCommissionSessionSequence>
{
    public void Configure(EntityTypeBuilder<TenderCommissionSessionSequence> b)
    {
        b.ToTable("TenderCommissionSessionSequences", DatabaseSchemas.Commission);
        b.ConfigureEntity();
        b.HasIndex(x => x.Year).IsUnique();
    }
}
