using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Infrastructure.Persistence;
using PetroProcure.Application.Mesc;
using PetroProcure.Infrastructure.Persistence.Repositories;
using PetroProcure.Application.Indents;
using PetroProcure.Application.PurchaseFiles;
using PetroProcure.Application.Documents;
using PetroProcure.Infrastructure.Storage;
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
using PetroProcure.Reporting;
using PetroProcure.AI;

namespace PetroProcure.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PetroProcureDb")
            ?? throw new InvalidOperationException("Connection string 'PetroProcureDb' was not found.");

        services.AddDbContext<PetroProcureDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(PetroProcureDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure();
            });
        });

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<PetroProcureDbContext>();

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IMescCatalogRepository, MescCatalogRepository>();
        services.AddScoped<IIndentRepository, IndentRepository>();
        services.AddScoped<IPurchaseFileRepository, PurchaseFileRepository>();
        services.AddScoped<IFileDocumentRepository, FileDocumentRepository>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IFileScanner, NoOpFileScanner>();
        services.AddScoped<IOrphanFileCleanupService, OrphanFileCleanupService>();
        services.AddScoped<IWorkflowRepository, WorkflowRepository>();
        services.AddScoped<IWorkflowActionRepository, WorkflowActionRepository>();
        services.AddScoped<IWorkflowActionResolver, WorkflowActionResolver>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IInquiryRepository, InquiryRepository>();
        services.AddScoped<IOrdersRepository, OrdersRepository>();
        services.AddScoped<ITenderRepository, TenderRepository>();
        services.AddScoped<ITenderNumberService, TenderNumberService>();
        services.AddScoped<ITenderEligibilityService, TenderEligibilityService>();
        services.AddScoped<ITenderComparisonService, TenderComparisonService>();
        services.AddScoped<ICommissionSessionRepository, CommissionSessionRepository>();
        services.AddScoped<ICommissionSessionNumberService, CommissionSessionNumberService>();
        services.AddScoped<ICommissionSessionEligibilityService, CommissionSessionEligibilityService>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<LegalRuleRepository>();
        services.AddScoped<ILegalRuleRepository>(sp => sp.GetRequiredService<LegalRuleRepository>());
        services.AddScoped<IPurchaseFileRuleContextBuilder>(sp => sp.GetRequiredService<LegalRuleRepository>());
        services.AddScoped<IReportDataProvider, ReportDataProvider>();
        services.AddScoped<AiRepository>();
        services.AddScoped<IAiEvaluationRepository>(sp => sp.GetRequiredService<AiRepository>());
        services.AddScoped<IPurchaseFileAiContextBuilder>(sp => sp.GetRequiredService<AiRepository>());
        services.AddHostedService<AdminBootstrapService>();

        services.AddHealthChecks()
            .AddDbContextCheck<PetroProcureDbContext>(
                name: "petroprocure-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "sqlserver", "ready"]);

        return services;
    }
}
