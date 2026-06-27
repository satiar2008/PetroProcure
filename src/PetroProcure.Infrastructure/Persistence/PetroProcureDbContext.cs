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
using PetroProcure.Domain.Modules.PurchaseOrders;
using PetroProcure.Domain.Modules.Suppliers;
using PetroProcure.Domain.Modules.Tenders;
using PetroProcure.Domain.Modules.TenderCommission;
using PetroProcure.Domain.Modules.Warehouse;
using PetroProcure.Domain.Modules.Workflow;
using PetroProcure.AI;
using PetroProcure.Domain.Modules.Ai;
using PetroProcure.Domain.Common;
using DomainAiEvaluationJob = PetroProcure.Domain.Modules.Ai.AiEvaluationJob;
using DomainAiEvaluationResult = PetroProcure.Domain.Modules.Ai.AiEvaluationResult;
using DomainAiFinding = PetroProcure.Domain.Modules.Ai.AiFinding;
using DomainAiRecommendation = PetroProcure.Domain.Modules.Ai.AiRecommendation;
using LegalDocument = PetroProcure.Domain.Modules.Legal.LegalDocument;
using LegalArticle = PetroProcure.Domain.Modules.Legal.LegalArticle;
using LegalClause = PetroProcure.Domain.Modules.Legal.LegalClause;
using LegalRuleAuditLog = PetroProcure.Domain.Modules.Legal.LegalRuleAuditLog;
using LegalProcurementRule = PetroProcure.Domain.Modules.Legal.ProcurementRule;
using LegalProcurementRuleEvaluation = PetroProcure.Domain.Modules.Legal.ProcurementRuleEvaluation;
using LegalProcurementRuleFinding = PetroProcure.Domain.Modules.Legal.ProcurementRuleFinding;
using LegalProcurementRuleSet = PetroProcure.Domain.Modules.Legal.ProcurementRuleSet;
using LegalProcurementRuleVersion = PetroProcure.Domain.Modules.Legal.ProcurementRuleVersion;

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
    public DbSet<PurchaseFileTechnicalReview> PurchaseFileTechnicalReviews => Set<PurchaseFileTechnicalReview>();

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
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<PurchaseOrderApproval> PurchaseOrderApprovals => Set<PurchaseOrderApproval>();
    public DbSet<PurchaseOrderDocument> PurchaseOrderDocuments => Set<PurchaseOrderDocument>();
    public DbSet<PurchaseOrderSequence> PurchaseOrderSequences => Set<PurchaseOrderSequence>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseReceipt> WarehouseReceipts => Set<WarehouseReceipt>();
    public DbSet<WarehouseReceiptItem> WarehouseReceiptItems => Set<WarehouseReceiptItem>();
    public DbSet<WarehouseReceiptDocument> WarehouseReceiptDocuments => Set<WarehouseReceiptDocument>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<WarehouseReceiptSequence> WarehouseReceiptSequences => Set<WarehouseReceiptSequence>();
    public DbSet<InventoryTransactionSequence> InventoryTransactionSequences => Set<InventoryTransactionSequence>();
    public DbSet<AiProvider> AiProviders => Set<AiProvider>();
    public DbSet<AiModel> AiModels => Set<AiModel>();
    public DbSet<AiAgentDefinition> AiAgentDefinitions => Set<AiAgentDefinition>();
    public DbSet<AiPromptTemplate> AiPromptTemplates => Set<AiPromptTemplate>();
    public DbSet<ProcurementRule> ProcurementRules => Set<ProcurementRule>();
    public DbSet<ProcurementRuleClause> ProcurementRuleClauses => Set<ProcurementRuleClause>();
    public DbSet<DomainAiEvaluationJob> AiEvaluationJobs => Set<DomainAiEvaluationJob>();
    public DbSet<DomainAiEvaluationResult> AiEvaluationResults => Set<DomainAiEvaluationResult>();
    public DbSet<DomainAiFinding> AiFindings => Set<DomainAiFinding>();
    public DbSet<DomainAiRecommendation> AiRecommendations => Set<DomainAiRecommendation>();
    public DbSet<AiConversation> AiConversations => Set<AiConversation>();
    public DbSet<AiMessage> AiMessages => Set<AiMessage>();
    public DbSet<AiAnalysisEvaluation> AiAnalysisEvaluations => Set<AiAnalysisEvaluation>();
    public DbSet<AiAnalysisFinding> AiAnalysisFindings => Set<AiAnalysisFinding>();
    public DbSet<AiAnalysisRecommendation> AiAnalysisRecommendations => Set<AiAnalysisRecommendation>();
    public DbSet<AiProviderRequestLog> AiProviderRequestLogs => Set<AiProviderRequestLog>();
    public DbSet<AiDocumentChunk> AiDocumentChunks => Set<AiDocumentChunk>();
    public DbSet<AiEmbedding> AiEmbeddings => Set<AiEmbedding>();
    public DbSet<LegalDocument> LegalDocuments => Set<LegalDocument>();
    public DbSet<LegalArticle> LegalArticles => Set<LegalArticle>();
    public DbSet<LegalClause> LegalClauses => Set<LegalClause>();
    public DbSet<LegalProcurementRuleSet> LegalProcurementRuleSets => Set<LegalProcurementRuleSet>();
    public DbSet<LegalProcurementRule> LegalProcurementRules => Set<LegalProcurementRule>();
    public DbSet<LegalProcurementRuleVersion> LegalProcurementRuleVersions => Set<LegalProcurementRuleVersion>();
    public DbSet<LegalProcurementRuleEvaluation> LegalProcurementRuleEvaluations => Set<LegalProcurementRuleEvaluation>();
    public DbSet<LegalProcurementRuleFinding> LegalProcurementRuleFindings => Set<LegalProcurementRuleFinding>();
    public DbSet<LegalRuleAuditLog> LegalRuleAuditLogs => Set<LegalRuleAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(DatabaseSchemas.Purchase);
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PetroProcureDbContext).Assembly);
    }
}
