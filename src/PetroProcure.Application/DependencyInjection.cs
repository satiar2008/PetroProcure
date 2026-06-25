using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        return services;
    }
}
