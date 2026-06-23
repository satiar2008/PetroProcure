using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetroProcure.Application.Mesc;
using PetroProcure.Application.Indents;
using PetroProcure.Application.PurchaseFiles;
using PetroProcure.Application.Documents;
using PetroProcure.Application.Workflow;

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
        return services;
    }
}
