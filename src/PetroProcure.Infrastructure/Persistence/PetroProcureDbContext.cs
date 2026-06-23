using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetroProcure.Domain.Modules.Security;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Domain.Modules.Documents;
using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.Items;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.Workflow;
using PetroProcure.AI;

namespace PetroProcure.Infrastructure.Persistence;

public sealed class PetroProcureDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public PetroProcureDbContext(DbContextOptions<PetroProcureDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<ApplicationUserProfile> ApplicationUserProfiles => Set<ApplicationUserProfile>();

    public DbSet<UserDepartment> UserDepartments => Set<UserDepartment>();

    public DbSet<DepartmentMenuItem> DepartmentMenuItems => Set<DepartmentMenuItem>();

    public DbSet<MescGeneralGroup> MescGeneralGroups => Set<MescGeneralGroup>();

    public DbSet<MescItem> MescItems => Set<MescItem>();

    public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();

    public DbSet<Indent> Indents => Set<Indent>();

    public DbSet<IndentItem> IndentItems => Set<IndentItem>();
    public DbSet<IndentSequence> IndentSequences => Set<IndentSequence>();

    public DbSet<PurchaseFile> PurchaseFiles => Set<PurchaseFile>();

    public DbSet<PurchaseFileItem> PurchaseFileItems => Set<PurchaseFileItem>();

    public DbSet<PurchaseFileStatusHistory> PurchaseFileStatusHistories => Set<PurchaseFileStatusHistory>();

    public DbSet<PurchaseFileNote> PurchaseFileNotes => Set<PurchaseFileNote>();
    public DbSet<PurchaseFileSequence> PurchaseFileSequences => Set<PurchaseFileSequence>();

    public DbSet<FileDocument> FileDocuments => Set<FileDocument>();

    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();

    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();

    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();

    public DbSet<InboxTask> InboxTasks => Set<InboxTask>();
    public DbSet<WorkflowActionDefinition> WorkflowActionDefinitions => Set<WorkflowActionDefinition>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<AuthRefreshToken> AuthRefreshTokens => Set<AuthRefreshToken>();
    public DbSet<AdminAuditLog> AdminAuditLogs => Set<AdminAuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<AiProvider> AiProviders => Set<AiProvider>();
    public DbSet<AiModel> AiModels => Set<AiModel>();
    public DbSet<AiAgentDefinition> AiAgentDefinitions => Set<AiAgentDefinition>();
    public DbSet<AiPromptTemplate> AiPromptTemplates => Set<AiPromptTemplate>();
    public DbSet<ProcurementRule> ProcurementRules => Set<ProcurementRule>();
    public DbSet<ProcurementRuleClause> ProcurementRuleClauses => Set<ProcurementRuleClause>();
    public DbSet<AiEvaluationJob> AiEvaluationJobs => Set<AiEvaluationJob>();
    public DbSet<AiEvaluationResult> AiEvaluationResults => Set<AiEvaluationResult>();
    public DbSet<AiFinding> AiFindings => Set<AiFinding>();
    public DbSet<AiRecommendation> AiRecommendations => Set<AiRecommendation>();
    public DbSet<AiConversation> AiConversations => Set<AiConversation>();
    public DbSet<AiMessage> AiMessages => Set<AiMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(DatabaseSchemas.Purchase);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PetroProcureDbContext).Assembly);
    }
}
