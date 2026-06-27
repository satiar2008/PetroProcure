using Microsoft.EntityFrameworkCore;
using PetroProcure.AI;
using PetroProcure.Infrastructure.Persistence.Configurations;
using PetroProcure.Domain.Modules.Ai;
using DomainAiEvaluationJob = PetroProcure.Domain.Modules.Ai.AiEvaluationJob;
using DomainAiEvaluationResult = PetroProcure.Domain.Modules.Ai.AiEvaluationResult;
using DomainAiFinding = PetroProcure.Domain.Modules.Ai.AiFinding;
using DomainAiRecommendation = PetroProcure.Domain.Modules.Ai.AiRecommendation;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Ai;

internal static class AiConfigurations
{
    public static void Configure(ModelBuilder b) { }
}
internal sealed class AiProviderConfiguration : IEntityTypeConfiguration<AiProvider> { public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AiProvider> b) { b.ToTable("AiProviders", DatabaseSchemas.Ai); b.ConfigureEntity(); b.Property(x => x.Name).HasMaxLength(200); b.Property(x => x.ProviderType).HasMaxLength(100); b.Property(x => x.BaseUrl).HasMaxLength(1000); } }
internal sealed class AiModelConfiguration : IEntityTypeConfiguration<AiModel> { public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AiModel> b) { b.ToTable("AiModels", DatabaseSchemas.Ai); b.ConfigureEntity(); b.Property(x => x.Name).HasMaxLength(200); b.Property(x => x.ModelIdentifier).HasMaxLength(300); } }
internal sealed class AiAgentDefinitionConfiguration : IEntityTypeConfiguration<AiAgentDefinition> { public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AiAgentDefinition> b) { b.ToTable("AiAgentDefinitions", DatabaseSchemas.Ai); b.ConfigureEntity(); b.Property(x => x.Name).HasMaxLength(200); b.Property(x => x.Capability).HasMaxLength(200); } }
internal sealed class AiPromptTemplateConfiguration : IEntityTypeConfiguration<AiPromptTemplate> { public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AiPromptTemplate> b) { b.ToTable("AiPromptTemplates", DatabaseSchemas.Ai); b.ConfigureEntity(); b.Property(x => x.Name).HasMaxLength(200); b.Property(x => x.Template).HasMaxLength(8000); } }
internal sealed class ProcurementRuleConfiguration : IEntityTypeConfiguration<ProcurementRule> { public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ProcurementRule> b) { b.ToTable("ProcurementRules", DatabaseSchemas.Ai); b.ConfigureEntity(); b.Property(x => x.Title).HasMaxLength(500); b.HasMany(x => x.Clauses).WithOne().HasForeignKey(x => x.ProcurementRuleId); b.Navigation(x => x.Clauses).UsePropertyAccessMode(PropertyAccessMode.Field); } }
internal sealed class ProcurementRuleClauseConfiguration : IEntityTypeConfiguration<ProcurementRuleClause> { public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ProcurementRuleClause> b) { b.ToTable("ProcurementRuleClauses", DatabaseSchemas.Ai); b.ConfigureEntity(); b.Property(x => x.ConditionType).HasMaxLength(100); b.Property(x => x.ConditionValue).HasMaxLength(1000); b.Property(x => x.ConditionDescription).HasMaxLength(2000); } }
internal sealed class AiEvaluationJobConfiguration : IEntityTypeConfiguration<DomainAiEvaluationJob>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<DomainAiEvaluationJob> b)
    {
        b.ToTable("AiEvaluationJobs", DatabaseSchemas.Ai);
        b.ConfigureEntity();
        b.Property(x => x.SourceSystem).HasMaxLength(100);
        b.Property(x => x.EntityType).HasMaxLength(100);
        b.Property(x => x.AnalysisType).HasMaxLength(100);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.CorrelationId).HasMaxLength(100);
        b.Property(x => x.ExternalJobId).HasMaxLength(200);
        b.Property(x => x.RequestJson).HasColumnType("nvarchar(max)");
        b.Property(x => x.ContextJson).HasColumnType("nvarchar(max)");
        b.Property(x => x.ResultJson).HasColumnType("nvarchar(max)");
        b.Property(x => x.ErrorMessage).HasMaxLength(2000);
        b.Property(x => x.LockedBy).HasMaxLength(200);
        b.HasIndex(x => new { x.Status, x.NextRetryAtUtc });
        b.HasIndex(x => x.CorrelationId).IsUnique();
        b.HasIndex(x => x.ExternalJobId).IsUnique().HasFilter("[ExternalJobId] IS NOT NULL");
        b.HasIndex(x => new { x.EntityType, x.EntityId });
    }
}
internal sealed class AiEvaluationResultConfiguration : IEntityTypeConfiguration<DomainAiEvaluationResult>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<DomainAiEvaluationResult> b)
    {
        b.ToTable("AiEvaluationResults", DatabaseSchemas.Ai);
        b.ConfigureEntity();
        b.Property(x => x.EntityType).HasMaxLength(100);
        b.Property(x => x.AnalysisType).HasMaxLength(100);
        b.Property(x => x.Summary).HasMaxLength(8000);
        b.Property(x => x.RawResultJson).HasColumnType("nvarchar(max)");
        b.Property(x => x.Model).HasMaxLength(200);
        b.Property(x => x.Provider).HasMaxLength(100);
        b.HasIndex(x => x.JobId).IsUnique();
        b.HasOne<DomainAiEvaluationJob>().WithMany().HasForeignKey(x => x.JobId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Findings).WithOne().HasForeignKey(x => x.ResultId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Recommendations).WithOne().HasForeignKey(x => x.ResultId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Findings).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.Recommendations).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
internal sealed class AiFindingConfiguration : IEntityTypeConfiguration<DomainAiFinding>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<DomainAiFinding> b)
    {
        b.ToTable("AiFindings", DatabaseSchemas.Ai);
        b.ConfigureEntity();
        b.Property(x => x.Title).HasMaxLength(500);
        b.Property(x => x.Description).HasMaxLength(4000);
        b.Property(x => x.Severity).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.RelatedClauseCode).HasMaxLength(100);
        b.HasIndex(x => x.ResultId);
    }
}
internal sealed class AiRecommendationConfiguration : IEntityTypeConfiguration<DomainAiRecommendation>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<DomainAiRecommendation> b)
    {
        b.ToTable("AiRecommendations", DatabaseSchemas.Ai);
        b.ConfigureEntity();
        b.Property(x => x.Title).HasMaxLength(500);
        b.Property(x => x.Description).HasMaxLength(4000);
        b.Property(x => x.Priority).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.SuggestedAction).HasMaxLength(300);
        b.HasIndex(x => x.ResultId);
    }
}
internal sealed class AiConversationConfiguration : IEntityTypeConfiguration<AiConversation> { public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AiConversation> b) { b.ToTable("AiConversations", DatabaseSchemas.Ai); b.ConfigureEntity(); b.Property(x => x.Title).HasMaxLength(500); } }
internal sealed class AiMessageConfiguration : IEntityTypeConfiguration<AiMessage> { public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AiMessage> b) { b.ToTable("AiMessages", DatabaseSchemas.Ai); b.ConfigureEntity(); b.Property(x => x.Role).HasMaxLength(50); b.Property(x => x.Content).HasMaxLength(8000); } }
internal sealed class AiAnalysisEvaluationConfiguration : IEntityTypeConfiguration<AiAnalysisEvaluation>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AiAnalysisEvaluation> b)
    {
        b.ToTable("AiAnalysisEvaluations", DatabaseSchemas.Ai);
        b.ConfigureEntity();
        b.Property(x => x.EntityType).HasMaxLength(100);
        b.Property(x => x.AnalysisType).HasMaxLength(100);
        b.Property(x => x.Provider).HasMaxLength(100);
        b.Property(x => x.Model).HasMaxLength(200);
        b.Property(x => x.Status).HasMaxLength(50);
        b.Property(x => x.PromptSummary).HasMaxLength(2000);
        b.Property(x => x.ResultSummary).HasMaxLength(8000);
        b.Property(x => x.RiskLevel).HasMaxLength(50);
        b.Property(x => x.ErrorMessage).HasMaxLength(2000);
        b.Property(x => x.MetadataJson).HasMaxLength(4000);
        b.HasIndex(x => new { x.EntityType, x.EntityId });
        b.HasIndex(x => x.CreatedAt);
    }
}
internal sealed class AiAnalysisFindingConfiguration : IEntityTypeConfiguration<AiAnalysisFinding>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AiAnalysisFinding> b)
    {
        b.ToTable("AiAnalysisFindings", DatabaseSchemas.Ai);
        b.ConfigureEntity();
        b.Property(x => x.Severity).HasMaxLength(50);
        b.Property(x => x.Title).HasMaxLength(500);
        b.Property(x => x.Description).HasMaxLength(4000);
        b.Property(x => x.Evidence).HasMaxLength(4000);
        b.Property(x => x.Recommendation).HasMaxLength(4000);
        b.Property(x => x.LegalReference).HasMaxLength(1000);
        b.HasOne<AiAnalysisEvaluation>().WithMany().HasForeignKey(x => x.EvaluationId).OnDelete(DeleteBehavior.Cascade);
    }
}
internal sealed class AiAnalysisRecommendationConfiguration : IEntityTypeConfiguration<AiAnalysisRecommendation>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AiAnalysisRecommendation> b)
    {
        b.ToTable("AiAnalysisRecommendations", DatabaseSchemas.Ai);
        b.ConfigureEntity();
        b.Property(x => x.Severity).HasMaxLength(50);
        b.Property(x => x.Title).HasMaxLength(500);
        b.Property(x => x.Description).HasMaxLength(4000);
        b.Property(x => x.RelatedAction).HasMaxLength(300);
        b.HasOne<AiAnalysisEvaluation>().WithMany().HasForeignKey(x => x.EvaluationId).OnDelete(DeleteBehavior.Cascade);
    }
}
internal sealed class AiProviderRequestLogConfiguration : IEntityTypeConfiguration<AiProviderRequestLog>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AiProviderRequestLog> b)
    {
        b.ToTable("AiProviderRequestLogs", DatabaseSchemas.Ai);
        b.ConfigureEntity();
        b.Property(x => x.Provider).HasMaxLength(100);
        b.Property(x => x.EntityType).HasMaxLength(100);
        b.Property(x => x.AnalysisType).HasMaxLength(100);
        b.Property(x => x.Status).HasMaxLength(50);
        b.Property(x => x.ErrorCode).HasMaxLength(100);
        b.Property(x => x.ErrorMessage).HasMaxLength(2000);
        b.Property(x => x.Cost).HasPrecision(18, 4);
        b.HasIndex(x => new { x.EntityType, x.EntityId });
        b.HasIndex(x => x.StartedAt);
    }
}
internal sealed class AiDocumentChunkConfiguration : IEntityTypeConfiguration<AiDocumentChunk>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AiDocumentChunk> b)
    {
        b.ToTable("AiDocumentChunks", DatabaseSchemas.Ai);
        b.ConfigureEntity();
        b.Property(x => x.SourceType).HasConversion<string>().HasMaxLength(100);
        b.Property(x => x.Text).HasColumnType("nvarchar(max)");
        b.Property(x => x.ContentHash).HasMaxLength(128);
        b.Property(x => x.MetadataJson).HasColumnType("nvarchar(max)");
        b.HasIndex(x => new { x.SourceType, x.SourceId, x.Ordinal }).IsUnique();
        b.HasIndex(x => x.PurchaseFileId);
        b.HasIndex(x => x.DocumentId);
        b.HasIndex(x => x.LegalClauseId);
        b.HasIndex(x => x.ContentHash);
    }
}
internal sealed class AiEmbeddingConfiguration : IEntityTypeConfiguration<AiEmbedding>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AiEmbedding> b)
    {
        b.ToTable("AiEmbeddings", DatabaseSchemas.Ai);
        b.ConfigureEntity();
        b.Property(x => x.Model).HasMaxLength(200);
        b.Property(x => x.VectorJson).HasColumnType("nvarchar(max)");
        b.Property(x => x.ContentHash).HasMaxLength(128);
        b.HasIndex(x => x.ChunkId).IsUnique();
        b.HasIndex(x => new { x.Model, x.Dimensions });
        b.HasOne<AiDocumentChunk>().WithMany().HasForeignKey(x => x.ChunkId).OnDelete(DeleteBehavior.Cascade);
    }
}
