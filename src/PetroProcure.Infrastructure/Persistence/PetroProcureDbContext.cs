using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetroProcure.Domain.Modules.Security;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Domain.Modules.Documents;
using PetroProcure.Domain.Modules.Contracts;
using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.Inquiries;
using PetroProcure.Domain.Modules.Items;
using PetroProcure.Domain.Modules.Orders;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.Suppliers;
using PetroProcure.Domain.Modules.Tenders;
using PetroProcure.Domain.Modules.TenderCommission;
using PetroProcure.Domain.Modules.Workflow;
using PetroProcure.AI;
using PetroProcure.Domain.Common;

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
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();
    public DbSet<InquiryItem> InquiryItems => Set<InquiryItem>();
    public DbSet<InquirySupplier> InquirySuppliers => Set<InquirySupplier>();
    public DbSet<SupplierQuote> SupplierQuotes => Set<SupplierQuote>();
    public DbSet<SupplierQuoteItem> SupplierQuoteItems => Set<SupplierQuoteItem>();
    public DbSet<InquiryDocument> InquiryDocuments => Set<InquiryDocument>();
    public DbSet<InquirySequence> InquirySequences => Set<InquirySequence>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierContact> SupplierContacts => Set<SupplierContact>();
    public DbSet<SupplierCategory> SupplierCategories => Set<SupplierCategory>();
    public DbSet<SupplierCategoryAssignment> SupplierCategoryAssignments => Set<SupplierCategoryAssignment>();
    public DbSet<SupplierDocument> SupplierDocuments => Set<SupplierDocument>();
    public DbSet<SupplierEvaluation> SupplierEvaluations => Set<SupplierEvaluation>();
    public DbSet<InventoryControlItem> InventoryControlItems => Set<InventoryControlItem>();
    public DbSet<StockBalance> StockBalances => Set<StockBalance>();
    public DbSet<MaterialNeed> MaterialNeeds => Set<MaterialNeed>();
    public DbSet<ShortageAlert> ShortageAlerts => Set<ShortageAlert>();
    public DbSet<MaterialNeedSequence> MaterialNeedSequences => Set<MaterialNeedSequence>();
    public DbSet<Tender> Tenders => Set<Tender>();
    public DbSet<TenderItem> TenderItems => Set<TenderItem>();
    public DbSet<TenderParticipant> TenderParticipants => Set<TenderParticipant>();
    public DbSet<TenderStage> TenderStages => Set<TenderStage>();
    public DbSet<TenderBid> TenderBids => Set<TenderBid>();
    public DbSet<TenderBidItem> TenderBidItems => Set<TenderBidItem>();
    public DbSet<TenderEvaluation> TenderEvaluations => Set<TenderEvaluation>();
    public DbSet<TenderDecision> TenderDecisions => Set<TenderDecision>();
    public DbSet<TenderDocument> TenderDocuments => Set<TenderDocument>();
    public DbSet<TenderSequence> TenderSequences => Set<TenderSequence>();
    public DbSet<TenderCommissionSession> TenderCommissionSessions => Set<TenderCommissionSession>();
    public DbSet<TenderCommissionMember> TenderCommissionMembers => Set<TenderCommissionMember>();
    public DbSet<TenderCommissionAgendaItem> TenderCommissionAgendaItems => Set<TenderCommissionAgendaItem>();
    public DbSet<TenderCommissionMinute> TenderCommissionMinutes => Set<TenderCommissionMinute>();
    public DbSet<TenderCommissionDecision> TenderCommissionDecisions => Set<TenderCommissionDecision>();
    public DbSet<TenderCommissionAttachment> TenderCommissionAttachments => Set<TenderCommissionAttachment>();
    public DbSet<TenderCommissionSessionSequence> TenderCommissionSessionSequences => Set<TenderCommissionSessionSequence>();
    public DbSet<PurchaseContract> PurchaseContracts => Set<PurchaseContract>();
    public DbSet<PurchaseContractItem> PurchaseContractItems => Set<PurchaseContractItem>();
    public DbSet<ContractClause> ContractClauses => Set<ContractClause>();
    public DbSet<ContractTemplate> ContractTemplates => Set<ContractTemplate>();
    public DbSet<ContractTemplateClause> ContractTemplateClauses => Set<ContractTemplateClause>();
    public DbSet<ContractApproval> ContractApprovals => Set<ContractApproval>();
    public DbSet<ContractDocument> ContractDocuments => Set<ContractDocument>();
    public DbSet<ContractSequence> ContractSequences => Set<ContractSequence>();
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
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PetroProcureDbContext).Assembly);
    }
}
