using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Legal;
using PetroProcure.Infrastructure.Persistence.Configurations;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Legal;

internal sealed class LegalDocumentConfiguration : IEntityTypeConfiguration<LegalDocument>
{
    public void Configure(EntityTypeBuilder<LegalDocument> b)
    {
        b.ToTable("LegalDocuments", DatabaseSchemas.Legal);
        b.ConfigureEntity();
        b.Property(x => x.Title).HasMaxLength(500);
        b.Property(x => x.OriginalFileName).HasMaxLength(260);
        b.Property(x => x.StoredFileName).HasMaxLength(260);
        b.Property(x => x.FileHash).HasMaxLength(128);
        b.Property(x => x.RelativePath).HasMaxLength(1000);
        b.Property(x => x.Extension).HasMaxLength(20);
        b.Property(x => x.MimeType).HasMaxLength(200);
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.SourceDocumentTitle).HasMaxLength(500);
        b.Property(x => x.SourceDocumentNumber).HasMaxLength(100);
        b.Property(x => x.SearchText).HasMaxLength(4000);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        b.HasMany(x => x.Articles).WithOne().HasForeignKey(x => x.LegalDocumentId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Articles).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasIndex(x => x.FileHash);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.IsDeleted);
    }
}

internal sealed class LegalArticleConfiguration : IEntityTypeConfiguration<LegalArticle>
{
    public void Configure(EntityTypeBuilder<LegalArticle> b)
    {
        b.ToTable("LegalArticles", DatabaseSchemas.Legal);
        b.ConfigureEntity();
        b.Property(x => x.ArticleNumber).HasMaxLength(50);
        b.Property(x => x.Title).HasMaxLength(500);
        b.Property(x => x.Body).HasMaxLength(8000);
        b.Property(x => x.SearchText).HasMaxLength(8000);
        b.HasMany(x => x.Clauses).WithOne().HasForeignKey(x => x.LegalArticleId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Clauses).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasIndex(x => new { x.LegalDocumentId, x.ArticleNumber }).IsUnique();
    }
}

internal sealed class LegalClauseConfiguration : IEntityTypeConfiguration<LegalClause>
{
    public void Configure(EntityTypeBuilder<LegalClause> b)
    {
        b.ToTable("LegalClauses", DatabaseSchemas.Legal);
        b.ConfigureEntity();
        b.Property(x => x.ClauseNumber).HasMaxLength(50);
        b.Property(x => x.Body).HasMaxLength(8000);
        b.Property(x => x.Note).HasMaxLength(4000);
        b.Property(x => x.AppliesTo).HasMaxLength(100);
        b.Property(x => x.Severity).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.Tags).HasMaxLength(1000);
        b.Property(x => x.SearchText).HasMaxLength(8000);
        b.HasIndex(x => new { x.LegalArticleId, x.ClauseNumber }).IsUnique();
        b.HasIndex(x => x.AppliesTo);
        b.HasIndex(x => x.Severity);
    }
}

internal sealed class ProcurementRuleSetConfiguration : IEntityTypeConfiguration<ProcurementRuleSet>
{
    public void Configure(EntityTypeBuilder<ProcurementRuleSet> b)
    {
        b.ToTable("ProcurementRuleSets", DatabaseSchemas.Rule);
        b.ConfigureEntity();
        b.Property(x => x.Name).HasMaxLength(300);
        b.Property(x => x.Description).HasMaxLength(2000);
    }
}

internal sealed class LegalProcurementRuleConfiguration : IEntityTypeConfiguration<ProcurementRule>
{
    public void Configure(EntityTypeBuilder<ProcurementRule> b)
    {
        b.ToTable("ProcurementRules", DatabaseSchemas.Rule);
        b.ConfigureEntity();
        b.Property(x => x.Code).HasMaxLength(100);
        b.Property(x => x.Title).HasMaxLength(500);
        b.HasMany(x => x.Versions).WithOne().HasForeignKey(x => x.ProcurementRuleId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Versions).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasIndex(x => x.Code).IsUnique();
    }
}

internal sealed class ProcurementRuleVersionConfiguration : IEntityTypeConfiguration<ProcurementRuleVersion>
{
    public void Configure(EntityTypeBuilder<ProcurementRuleVersion> b)
    {
        b.ToTable("ProcurementRuleVersions", DatabaseSchemas.Rule);
        b.ConfigureEntity();
        b.Property(x => x.Title).HasMaxLength(500);
        b.Property(x => x.RuleType).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.Severity).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.EvaluationMode).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.LegalReference).HasMaxLength(500);
        b.Property(x => x.ConditionType).HasMaxLength(100);
        b.Property(x => x.ConditionValue).HasMaxLength(1000);
        b.Property(x => x.ConditionDescription).HasMaxLength(2000);
        b.Property(x => x.SearchText).HasMaxLength(8000);
        b.HasIndex(x => new { x.ProcurementRuleId, x.VersionNo }).IsUnique();
        b.HasIndex(x => x.Status);
    }
}

internal sealed class ProcurementRuleEvaluationConfiguration : IEntityTypeConfiguration<ProcurementRuleEvaluation>
{
    public void Configure(EntityTypeBuilder<ProcurementRuleEvaluation> b)
    {
        b.ToTable("ProcurementRuleEvaluations", DatabaseSchemas.Rule);
        b.ConfigureEntity();
        b.Property(x => x.EntityType).HasMaxLength(100);
        b.Property(x => x.Summary).HasMaxLength(8000);
        b.HasMany(x => x.Findings).WithOne().HasForeignKey(x => x.ProcurementRuleEvaluationId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Findings).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasIndex(x => x.PurchaseFileId);
        b.HasIndex(x => x.TenderId);
    }
}

internal sealed class ProcurementRuleFindingConfiguration : IEntityTypeConfiguration<ProcurementRuleFinding>
{
    public void Configure(EntityTypeBuilder<ProcurementRuleFinding> b)
    {
        b.ToTable("ProcurementRuleFindings", DatabaseSchemas.Rule);
        b.ConfigureEntity();
        b.Property(x => x.Result).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.Severity).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.Title).HasMaxLength(500);
        b.Property(x => x.Description).HasMaxLength(4000);
        b.Property(x => x.LegalReference).HasMaxLength(500);
        b.HasIndex(x => x.RuleVersionId);
    }
}

internal sealed class LegalRuleAuditLogConfiguration : IEntityTypeConfiguration<LegalRuleAuditLog>
{
    public void Configure(EntityTypeBuilder<LegalRuleAuditLog> b)
    {
        b.ToTable("LegalRuleAuditLogs", DatabaseSchemas.Audit);
        b.ConfigureEntity();
        b.Property(x => x.EntityType).HasMaxLength(100);
        b.Property(x => x.Action).HasMaxLength(100);
        b.Property(x => x.Summary).HasMaxLength(4000);
        b.HasIndex(x => new { x.EntityType, x.EntityId });
    }
}
