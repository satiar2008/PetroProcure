using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PetroProcure.Application.Mesc;
using PetroProcure.Application.Indents;
using PetroProcure.Application.PurchaseFiles;
using PetroProcure.Application.Documents;
using PetroProcure.Application.Workflow;
using PetroProcure.Application.Suppliers;
using PetroProcure.Application.Inquiries;
using PetroProcure.Application.Orders;
using PetroProcure.Application.Tenders;
using PetroProcure.Application.Commission;
using PetroProcure.Application.Contracts;
using PetroProcure.Application.PurchaseOrders;
using PetroProcure.Application.Warehouse;
using PetroProcure.Application.Legal;
using PetroProcure.Application.Rag;
using PetroProcure.Application.Ai;

namespace PetroProcure.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MescCatalogOptions>(configuration.GetSection(MescCatalogOptions.SectionName));
        services.AddScoped<MescCommandHandler>();
        services.AddScoped<MescQueryHandler>();
        services.AddScoped<IIndentNumberService, IndentNumberService>();
        services.AddScoped<IndentCommandHandler>();
        services.AddScoped<IndentQueryHandler>();
        services.AddScoped<IPurchaseFileNumberService, PurchaseFileNumberService>();
        services.AddScoped<PurchaseFileCommandHandler>();
        services.AddScoped<PurchaseFileQueryHandler>();
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.AddScoped<WorkflowCommandHandler>();
        services.AddScoped<WorkflowQueryHandler>();
        services.AddScoped<SupplierCommandHandler>();
        services.AddScoped<SupplierQueryHandler>();
        services.AddScoped<IInquiryNumberService, InquiryNumberService>();
        services.AddScoped<InquiryCommandHandler>();
        services.AddScoped<InquiryQueryHandler>();
        services.AddScoped<OrdersCommandHandler>();
        services.AddScoped<OrdersQueryHandler>();
        services.AddScoped<TenderCommandHandler>();
        services.AddScoped<TenderQueryHandler>();
        services.AddScoped<CommissionCommandHandler>();
        services.AddScoped<CommissionQueryHandler>();
        services.AddScoped<IContractNumberService, ContractNumberService>();
        services.AddScoped<IContractEligibilityService, ContractEligibilityService>();
        services.AddScoped<IContractTemplateService, ContractTemplateService>();
        services.AddScoped<IContractReportDataSourceBuilder, ContractReportDataSourceBuilder>();
        services.AddScoped<ContractCommandHandler>();
        services.AddScoped<ContractQueryHandler>();
        services.AddScoped<IPurchaseOrderNumberService, PurchaseOrderNumberService>();
        services.AddScoped<IPurchaseOrderEligibilityService, PurchaseOrderEligibilityService>();
        services.AddScoped<IPurchaseOrderReportDataSourceBuilder, PurchaseOrderReportDataSourceBuilder>();
        services.AddScoped<PurchaseOrderCommandHandler>();
        services.AddScoped<PurchaseOrderQueryHandler>();
        services.AddScoped<IWarehouseReceiptNumberService, WarehouseReceiptNumberService>();
        services.AddScoped<IInventoryTransactionNumberService, InventoryTransactionNumberService>();
        services.AddScoped<IWarehouseReceiptEligibilityService, WarehouseReceiptEligibilityService>();
        services.AddScoped<IInventoryBalanceService, InventoryBalanceService>();
        services.AddScoped<WarehouseCommandHandler>();
        services.AddScoped<WarehouseQueryHandler>();
        services.AddScoped<LegalRuleCommandHandler>();
        services.AddScoped<LegalRuleQueryHandler>();
        services.AddScoped<LegalRuleEvaluationHandler>();
        services.AddScoped<ILegalClauseSearchService, LegalClauseSearchService>();
        services.AddScoped<PetroProcure.Application.Legal.IConditionEvaluator, PetroProcure.Application.Legal.JsonRuleConditionEvaluator>();
        services.AddScoped<PetroProcure.Application.Legal.IProcurementRuleImportService, PetroProcure.Application.Legal.ProcurementRuleImportService>();
        services.AddScoped<PetroProcure.Application.Legal.IProcurementRuleEvaluator, HybridProcurementRuleEvaluator>();
        services.AddScoped<IAiRuleExplanationService, MockAiRuleExplanationService>();
        // Fallback only: AddPetroProcureAi replaces this with AiCoreLegalEvaluationService in production.
        services.TryAddScoped<IAiLegalEvaluationService, NullAiLegalEvaluationService>();
        services.AddScoped<IProcurementRuleGateService, ProcurementRuleGateService>();
        services.AddScoped<IAiAuditService, LoggingAiAuditService>();
        services.AddScoped<IAiJobNotifier, NullAiJobNotifier>();
        services.AddScoped<IAiJobQueueService, AiJobQueueService>();
        services.AddScoped<IAiCoreCallbackService, AiCoreCallbackService>();
        services.AddScoped<IGroundedAiAnalysisService, GroundedAiAnalysisService>();
        // Fallback only: AddPetroProcureAi replaces this with AiCoreGroundedAnswerGenerator in production.
        services.TryAddScoped<IGroundedAiAnswerGenerator, NullGroundedAiAnswerGenerator>();
        services.Configure<PurchaseFileAiContextOptions>(configuration.GetSection(PurchaseFileAiContextOptions.SectionName));
        services.AddScoped<IPurchaseFileAiContextBuilder, PurchaseFileAiContextBuilder>();
        services.Configure<RagOptions>(configuration.GetSection(RagOptions.SectionName));
        services.AddScoped<IChunkingService, ChunkingService>();
        services.AddScoped<IRagSearchService, RagSearchService>();
        services.AddScoped<IRagRetriever, RagRetriever>();
        services.AddScoped<IRagIngestionQueue, RagIngestionQueue>();
        services.AddScoped<IRagIngestionService, RagIngestionService>();
        services.AddScoped<IRagMaintenanceService, RagMaintenanceService>();
        services.AddScoped<IRagQualityEvaluator, RagQualityEvaluator>();
        services.AddMemoryCache();
        return services;
    }
}
